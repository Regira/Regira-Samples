namespace Blog.Api.Data.Seeding;

/// <summary>Hand-written vocabularies the seeder draws correlated tuples from (category -> topics -> tags).</summary>
public static class SeedCatalog
{
    public record CategorySeed(string Title, string Color, string Description, string[] Topics, string[] Tags);

    public static readonly CategorySeed[] Categories =
    [
        new("Technology", "#2563eb", "Software, tools and the craft of building for the web.",
            ["TypeScript", "edge computing", "local-first software", "API design", "home automation", "open-source maintenance", "developer tooling", "web performance"],
            ["javascript", "dotnet", "web", "open-source", "tools"]),
        new("Design", "#db2777", "Typography, interfaces and the small decisions that make things feel right.",
            ["typography", "design systems", "colour theory", "accessibility", "editorial layout", "icon design", "micro-interactions"],
            ["ux", "css", "accessibility", "typography"]),
        new("Travel", "#0891b2", "Slow journeys, night trains and places worth the detour.",
            ["slow travel", "night trains", "Lisbon", "the Scottish Highlands", "Kyoto", "packing light", "the Alps by rail"],
            ["europe", "asia", "rail", "city-guide"]),
        new("Food & Drink", "#ea580c", "Seasonal cooking, patient baking and the occasional good bottle.",
            ["sourdough", "fermentation", "seasonal cooking", "natural wine", "coffee brewing", "weeknight dinners"],
            ["recipes", "baking", "coffee", "seasonal"]),
        new("Culture", "#7c3aed", "Film, music, art and the conversations around them.",
            ["independent cinema", "vinyl records", "modern poetry", "museum design", "literary podcasts", "public libraries"],
            ["film", "music", "art", "poetry"]),
        new("Science", "#059669", "Curious findings, explained without the jargon.",
            ["urban ecology", "sleep research", "citizen science", "climate data", "amateur astronomy", "the microbiome"],
            ["research", "climate", "space", "nature"]),
        new("Business", "#b45309", "Running small companies, remote teams and honest numbers.",
            ["remote teams", "pricing strategy", "bootstrapping", "writing proposals", "customer interviews", "small-shop accounting"],
            ["startups", "productivity", "leadership", "remote-work"]),
        new("Wellbeing", "#16a34a", "Movement, rest and a calmer relationship with our screens.",
            ["running", "digital minimalism", "mindful mornings", "strength training", "walking meetings", "better sleep"],
            ["fitness", "mindfulness", "sleep", "habits"]),
        new("Photography", "#475569", "Light, film and the patience of a good frame.",
            ["film photography", "golden hour", "portrait lighting", "photo editing", "street photography"],
            ["35mm", "lightroom", "portraits", "street"]),
        new("Books", "#9f1239", "Reading lists, reviews and the pleasure of a long novel.",
            ["reading habits", "translated fiction", "annotating books", "short stories", "reading lists"],
            ["fiction", "non-fiction", "classics", "book-review"]),
    ];

    /// <summary>Topics whose tags must agree with them (a Kyoto story is never tagged "europe").</summary>
    public static readonly Dictionary<string, string[]> TopicTags = new()
    {
        ["slow travel"] = ["europe", "rail", "city-guide"],
        ["night trains"] = ["europe", "rail"],
        ["Lisbon"] = ["europe", "city-guide"],
        ["the Scottish Highlands"] = ["europe"],
        ["Kyoto"] = ["asia", "city-guide"],
        ["packing light"] = ["city-guide"],
        ["the Alps by rail"] = ["europe", "rail"],
        ["amateur astronomy"] = ["space", "nature"],
        ["climate data"] = ["climate", "research"],
        ["urban ecology"] = ["nature", "climate"],
        ["sleep research"] = ["research"],
        ["the microbiome"] = ["research", "nature"],
        ["citizen science"] = ["research", "nature"],
        ["sourdough"] = ["baking", "recipes"],
        ["coffee brewing"] = ["coffee"],
        ["seasonal cooking"] = ["seasonal", "recipes"],
        ["weeknight dinners"] = ["recipes", "seasonal"],
        ["fermentation"] = ["recipes"],
        ["natural wine"] = ["seasonal"],
        ["independent cinema"] = ["film"],
        ["vinyl records"] = ["music"],
        ["modern poetry"] = ["poetry"],
        ["museum design"] = ["art"],
        ["better sleep"] = ["sleep", "habits"],
        ["running"] = ["fitness", "habits"],
        ["strength training"] = ["fitness", "habits"],
        ["mindful mornings"] = ["mindfulness", "habits"],
        ["digital minimalism"] = ["mindfulness", "habits"],
        ["film photography"] = ["35mm"],
        ["photo editing"] = ["lightroom"],
        ["portrait lighting"] = ["portraits"],
        ["street photography"] = ["street", "35mm"],
        ["TypeScript"] = ["javascript", "web", "tools"],
        ["web performance"] = ["web", "javascript"],
        ["typography"] = ["typography", "css"],
        ["accessibility"] = ["accessibility", "ux"],
        ["translated fiction"] = ["fiction", "book-review"],
        ["short stories"] = ["fiction"],
    };

    public static readonly string[] GeneralTags =
        ["how-to", "opinion", "long-read", "interview", "beginner", "deep-dive", "case-study", "checklist", "review", "essay", "guide", "weekend", "series"];

    public static readonly string[] TitleTemplates =
    [
        "A Beginner's Guide to {T}",
        "What {n} Years of {T} Taught Me",
        "Why {T} Deserves Your Attention",
        "The Quiet Art of {T}",
        "{T}: What Nobody Tells You",
        "How We Rethought {T}",
        "Notes on {T}",
        "Everything I Got Wrong About {T}",
        "{n} Lessons From a Year of {T}",
        "The Case for {T}",
        "Getting Started With {T} in {Y}",
        "In Defence of {T}",
        "A Field Guide to {T}",
        "The Surprising History of {T}",
        "Small Habits for Better {T}",
        "{T}, Slowly",
        "Rethinking {T} From First Principles",
        "What I Learned Trying {T} for 30 Days",
        "The Tools I Use for {T}",
        "An Honest Look at {T}",
    ];

    public static readonly string[] SummaryTemplates =
    [
        "A practical, unhurried look at {t} - what works, what doesn't and where to begin.",
        "Some hard-won lessons about {t}, collected over many quiet evenings and a few loud mistakes.",
        "Why {t} matters more than it seems, and how to make room for it in an ordinary week.",
        "A personal account of getting into {t}, with the notes I wish I had on day one.",
        "The ideas, habits and small tools that changed how I think about {t}.",
        "An essay on {t}: the popular advice, the overlooked details and a simpler way forward.",
    ];

    public static readonly string[] Sentences =
    [
        "Most conversations about {t} start in the wrong place.",
        "I came to {t} almost by accident, and it has stayed with me ever since.",
        "The first thing you notice about {t} is how much of it happens quietly, in the margins.",
        "There is a popular idea that {t} is only for specialists; in practice it rewards curiosity far more than expertise.",
        "What changed my mind was a small experiment that took less than a week.",
        "The details matter, but not in the way the tutorials suggest.",
        "It is tempting to buy your way into {t}, yet the best results usually come from patience.",
        "A good routine beats a perfect plan every single time.",
        "Friends who tried it with me reported the same pattern: slow progress, then a sudden click.",
        "The numbers are less interesting than the habits behind them.",
        "If you only take one thing from this piece, let it be this: start smaller than feels reasonable.",
        "Nobody gets this right the first time, and that is exactly the point.",
        "Looking back, the biggest mistakes were decisions I made too early.",
        "There is real joy in doing something carefully, even when nobody is watching.",
        "The community around {t} is generous, opinionated and occasionally exhausting.",
        "Much of the advice online is written for a situation you are probably not in.",
        "Constraints turned out to be a feature rather than a limitation.",
        "Every few months I revisit my assumptions, and every time a few of them fall away.",
        "The difference between good and great is usually a dozen tiny choices.",
        "It helps to keep notes, because memory is a generous but unreliable editor.",
        "{T} rewards people who ask simple questions and listen carefully to the answers.",
        "In the end, the craft is less about talent and more about showing up.",
        "Some weeks it felt like nothing moved at all.",
        "Then, almost without warning, the pieces started to fit together.",
        "I am still learning, and I expect to be for a long time.",
        "The best sessions were the ones I almost skipped.",
        "It is worth asking what problem you are really trying to solve before reaching for a new tool.",
        "Once the basics became automatic, there was finally room to enjoy the process.",
    ];

    public static readonly string[] Headings =
    [
        "Where it started", "The first mistakes", "What actually worked", "A closer look", "The tools that matter",
        "What I would do differently", "Common pitfalls", "Why it matters", "Getting started", "A short checklist",
        "The part nobody mentions", "Making it stick",
    ];

    public static readonly string[] ListItems =
    [
        "Start with one small, repeatable step", "Write down what you notice", "Ask someone more experienced for feedback",
        "Give it at least a month before judging", "Keep the setup simple", "Share what you learn",
        "Protect a regular time slot for it", "Measure less, observe more",
    ];

    public static readonly string[] Quotes =
    [
        "The best time to start was yesterday; the second best is a quiet Tuesday morning.",
        "Slow is smooth, and smooth is fast.",
        "Good work is mostly the removal of unnecessary things.",
        "You do not need permission to be curious.",
        "Care is visible, even in the smallest details.",
    ];
}
