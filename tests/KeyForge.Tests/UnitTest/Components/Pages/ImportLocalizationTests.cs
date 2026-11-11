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

        Assert.Contains($"T(\"{persian}\", \"{english}\")", source, StringComparison.Ordinal);
        Assert.DoesNotContain("????", source, StringComparison.Ordinal);
        Assert.DoesNotContain("????", navigation, StringComparison.Ordinal);
        Assert.Contains("ورود MusicXML", navigation, StringComparison.Ordinal);
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
