namespace KeyForge.Tests.UnitTest.Infrastructure.Content;

using KeyForge.Infrastructure.Content.Import;

public sealed class MusicXmlPerformanceImporterTests
{
    public const string OriginalXml = """
        <score-partwise version="4.0">
          <part-list><score-part id="P1"><part-name>Student</part-name></score-part><score-part id="P2"><part-name>Teacher</part-name></score-part></part-list>
          <part id="P1">
            <measure number="1"><attributes><divisions>2</divisions></attributes><direction><sound tempo="60"/></direction>
              <note><pitch><step>C</step><octave>4</octave></pitch><duration>2</duration><voice>1</voice></note>
              <note><chord/><pitch><step>E</step><octave>4</octave></pitch><duration>2</duration><voice>1</voice></note>
              <backup><duration>2</duration></backup>
              <note><pitch><step>C</step><octave>3</octave></pitch><duration>4</duration><voice>2</voice></note>
            </measure>
            <measure number="2"><note><rest/><duration>2</duration><voice>1</voice></note>
              <note><pitch><step>F</step><alter>1</alter><octave>4</octave></pitch><duration>2</duration><voice>1</voice></note>
            </measure>
          </part>
          <part id="P2"><measure number="1"><attributes><divisions>1</divisions></attributes><note><pitch><step>G</step><octave>5</octave></pitch><duration>4</duration></note></measure></part>
        </score-partwise>
        """;

    [Fact]
    public void ExplicitPartSelectionPreservesChordsVoicesRestsAndQuarterPulseDurations()
    {
        var importer = new MusicXmlPerformanceImporter();
        Assert.Equal(2, importer.GetParts(OriginalXml).Count);
        var lesson = new YamlLessonParser().Parse(importer.Import(OriginalXml, "P1", "source-test", "student", 90));
        var definition = Assert.IsType<SequenceExerciseDefinition>(Assert.Single(lesson.Exercises));
        Assert.Equal(60, definition.Tempo!.DefaultBpm);
        var events = definition.Performance!.Events;
        Assert.Equal(new[] { "C4", "E4", "C3", null, "F#4" }, events.Select(e => e.Pitch));
        Assert.Equal(new[] { 0d, 0, 0, 2, 3 }, events.Select(e => e.OnsetBeats));
        Assert.Equal(new[] { 1d, 1, 2, 1, 1 }, events.Select(e => e.DurationBeats));
        Assert.All(events, e => Assert.Equal(PracticeHand.Either, e.Hand));
        var teacher = new YamlLessonParser().Parse(importer.Import(OriginalXml, "P2", "teacher-test", "teacher", 90));
        Assert.Equal("G5", Assert.IsType<SequenceExerciseDefinition>(teacher.Exercises[0]).Performance!.Events[0].Pitch);
    }

    [Theory]
    [InlineData("<duration>2</duration>", "<duration>0</duration>")]
    [InlineData("<backup><duration>2</duration></backup>", "<backup><duration>20</duration></backup>")]
    [InlineData("<chord/>", "<chord/><tie type=\"start\"/>")]
    [InlineData("<sound tempo=\"60\"/>", "<sound tempo=\"60\"/><sound tempo=\"70\"/>")]
    public void UnsupportedOrMalformedInputFailsExplicitly(string before, string after) =>
        Assert.Throws<InvalidDataException>(() => new MusicXmlPerformanceImporter().Import(OriginalXml.Replace(before, after), "P1", "test", "exercise", 60));

    [Fact]
    public void MissingPartAndExternalEntitiesAreRejected()
    {
        var importer = new MusicXmlPerformanceImporter();
        Assert.Throws<InvalidDataException>(() => importer.Import(OriginalXml, "missing", "test", "exercise", 60));
        Assert.Throws<InvalidDataException>(() => importer.GetParts("<!DOCTYPE score-partwise [<!ENTITY x SYSTEM 'file:///does-not-exist'>]><score-partwise>&x;</score-partwise>"));
        Assert.Throws<InvalidDataException>(() => importer.GetParts("<score-timewise/>"));
    }
}
