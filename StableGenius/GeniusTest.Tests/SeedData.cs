using GeniusTest.Api.Data.Seeding;
using GeniusTest.Api.Entities.Questions;
using GeniusTest.Api.Entities.Reactions;
using GeniusTest.Api.Services.Spin;

namespace GeniusTest.Tests;

/// <summary>The shipped CSV seed files, read exactly the way the seeder reads them.</summary>
public static class SeedData
{
    public static string ContentRoot { get; } = FindContentRoot();

    public static List<Reaction> Reactions()
    {
        var id = 0;
        var items = SeedFiles.ReadReactions(ContentRoot).GetAwaiter().GetResult();
        items.ForEach(r => r.Id = ++id);
        return items;
    }

    public static List<Question> Questions() => SeedFiles.ReadQuestions(ContentRoot).GetAwaiter().GetResult();

    public static GameContent Content() => GameContent.Load(ContentRoot).GetAwaiter().GetResult();

    private static string FindContentRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "GeniusTest.Api")))
            dir = dir.Parent;
        return Path.Combine(dir!.FullName, "GeniusTest.Api");
    }
}
