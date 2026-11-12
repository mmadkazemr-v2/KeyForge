namespace KeyForge.Tests.UnitTest.Components.Pages;

public sealed class ImportLocalizationTests
{
    [Theory]
    [InlineData("وارد کردن بخش هنرجو از MusicXML", "Import a MusicXML learner part")]
    [InlineData("انتخاب یک بخش", "Select a part")]
    public void ImportPage_ContainsLocalizedLabelsWithoutReplacementText(
        string persian,
        string english)
    {
        var source = File.ReadAllText(FindImportPage());
        var navigation = File.ReadAllText(FindNavMenu());

        Assert.Contains(persian, source, StringComparison.Ordinal);
        Assert.Contains(english, source, StringComparison.Ordinal);
        Assert.Contains("پسوند فایل باید .xml یا .musicxml باشد.", source, StringComparison.Ordinal);
        Assert.DoesNotContain("????", source, StringComparison.Ordinal);
        Assert.DoesNotContain("????", navigation, StringComparison.Ordinal);
        Assert.Contains("ورود MusicXML", navigation, StringComparison.Ordinal);
    }

    [Fact]
    public void PerformanceComponents_ContainPersianAndEnglishWithoutQuestionMarkPlaceholders()
    {
        var workspace = File.ReadAllText(FindSourceFile(Path.Combine("Components", "Practice", "PerformanceWorkspace.razor")));
        var feedback = File.ReadAllText(FindSourceFile(Path.Combine("Components", "Practice", "PerformanceFeedback.razor")));
        Assert.Contains("Timing is relative to your first attack", workspace, StringComparison.Ordinal);
        Assert.Contains("زمان‌بندی از نخستین نواختن", workspace, StringComparison.Ordinal);
        Assert.Contains("Release correct", feedback, StringComparison.Ordinal);
        Assert.Contains("شدت صحیح", feedback, StringComparison.Ordinal);
        Assert.DoesNotContain("????", workspace, StringComparison.Ordinal);
        Assert.DoesNotContain("????", feedback, StringComparison.Ordinal);
    }

    private static string FindImportPage()
        => FindSourceFile(Path.Combine("Components", "Pages", "Import.razor"));

    private static string FindNavMenu()
        => FindSourceFile(Path.Combine("Components", "Layout", "NavMenu.razor"));

    private static string FindSourceFile(string relativePath)
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            var path = Path.Combine(directory.FullName, "src", "KeyForge", relativePath);
            if (File.Exists(path))
                return path;
        }

        throw new DirectoryNotFoundException($"Could not locate {relativePath}.");
    }
}
