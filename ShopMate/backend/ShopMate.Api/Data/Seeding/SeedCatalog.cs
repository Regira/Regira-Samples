namespace ShopMate.Api.Data.Seeding;

/// <summary>Static reference data for the seeder: the category DAG and a product catalog.</summary>
public static class SeedCatalog
{
    public record CategorySeed(string Title, string Icon, string Color, string? Description, params string[] Parents);

    // Roots first, then children (a child may name several parents -> multi-parent hierarchy)
    public static readonly CategorySeed[] Categories =
    [
        new("Fresh", "\U0001f96c", "#43a047", "Fresh produce"),
        new("Dairy & Eggs", "\U0001f95b", "#90caf9", "Milk, cheese, yogurt and eggs"),
        new("Bakery", "\U0001f956", "#d7a86e", "Bread and pastry"),
        new("Meat & Fish", "\U0001f969", "#e53935", "Butcher and fishmonger"),
        new("Deli", "\U0001f9c0", "#fbc02d", "Delicatessen counter"),
        new("Pantry", "\U0001f96b", "#8d6e63", "Dry and canned goods"),
        new("Frozen", "\U0001f9ca", "#4fc3f7", "Freezer aisle"),
        new("Drinks", "\U0001f964", "#1e88e5", "Beverages"),
        new("Snacks & Sweets", "\U0001f36b", "#8e24aa", "Treats"),
        new("Household", "\U0001f9fd", "#607d8b", "Cleaning and home supplies"),
        new("Personal Care", "\U0001f9f4", "#ec407a", "Hygiene and beauty"),
        new("Baby", "\U0001f37c", "#ffb74d", "Baby supplies"),
        new("Pets", "\U0001f43e", "#795548", "Pet supplies"),
        new("Breakfast", "\U0001f963", "#ff8f00", "Everything for the morning"),

        new("Fruit", "\U0001f34e", "#c62828", null, "Fresh"),
        new("Vegetables", "\U0001f955", "#2e7d32", null, "Fresh"),
        new("Herbs", "\U0001f33f", "#66bb6a", null, "Fresh"),
        new("Milk", "\U0001f95b", "#bbdefb", null, "Dairy & Eggs", "Breakfast"),
        new("Cheese", "\U0001f9c0", "#ffca28", null, "Dairy & Eggs", "Deli"),
        new("Yogurt", "\U0001f376", "#e3f2fd", null, "Dairy & Eggs", "Breakfast"),
        new("Eggs", "\U0001f95a", "#fff3e0", null, "Dairy & Eggs", "Breakfast"),
        new("Butter & Spreads", "\U0001f9c8", "#ffe082", null, "Dairy & Eggs", "Breakfast"),
        new("Bread", "\U0001f35e", "#bcaaa4", null, "Bakery", "Breakfast"),
        new("Pastry", "\U0001f950", "#ffcc80", null, "Bakery", "Snacks & Sweets"),
        new("Meat", "\U0001f969", "#b71c1c", null, "Meat & Fish"),
        new("Poultry", "\U0001f357", "#ef9a9a", null, "Meat & Fish"),
        new("Fish & Seafood", "\U0001f41f", "#0288d1", null, "Meat & Fish"),
        new("Cold Cuts", "\U0001f953", "#f48fb1", null, "Meat & Fish", "Deli"),
        new("Pasta & Rice", "\U0001f35d", "#ffb300", null, "Pantry"),
        new("Canned Goods", "\U0001f96b", "#a1887f", null, "Pantry"),
        new("Sauces & Condiments", "\U0001f345", "#d84315", null, "Pantry"),
        new("Spices", "\U0001f9c2", "#ff7043", null, "Pantry"),
        new("Cereals", "\U0001f963", "#ffa726", null, "Pantry", "Breakfast"),
        new("Baking", "\U0001f9c1", "#f8bbd0", null, "Pantry"),
        new("Frozen Vegetables", "\U0001f966", "#81c784", null, "Frozen", "Vegetables"),
        new("Ice Cream", "\U0001f368", "#ce93d8", null, "Frozen", "Snacks & Sweets"),
        new("Frozen Meals", "\U0001f355", "#ffab91", null, "Frozen"),
        new("Water", "\U0001f4a7", "#81d4fa", null, "Drinks"),
        new("Soft Drinks", "\U0001f964", "#e57373", null, "Drinks"),
        new("Juice", "\U0001f9c3", "#ffb74d", null, "Drinks", "Fruit", "Breakfast"),
        new("Coffee & Tea", "\u2615", "#6d4c41", null, "Drinks", "Breakfast"),
        new("Beer & Wine", "\U0001f377", "#880e4f", null, "Drinks"),
        new("Chips", "\U0001f954", "#fdd835", null, "Snacks & Sweets"),
        new("Chocolate", "\U0001f36b", "#5d4037", null, "Snacks & Sweets"),
        new("Cookies", "\U0001f36a", "#a1887f", null, "Snacks & Sweets", "Bakery"),
        new("Nuts", "\U0001f95c", "#bf8040", null, "Snacks & Sweets", "Pantry"),
        new("Cleaning", "\U0001f9f9", "#78909c", null, "Household"),
        new("Laundry", "\U0001f9fa", "#90a4ae", null, "Household"),
        new("Paper Goods", "\U0001f9fb", "#cfd8dc", null, "Household"),
        new("Oral Care", "\U0001faa5", "#4dd0e1", null, "Personal Care"),
        new("Hair Care", "\U0001f487", "#f06292", null, "Personal Care"),
        new("Hygiene", "\U0001f9fc", "#ba68c8", null, "Personal Care"),
        new("Diapers", "\U0001f476", "#ffe0b2", null, "Baby", "Paper Goods"),
        new("Baby Food", "\U0001f37c", "#ffcc80", null, "Baby"),
        new("Pet Food", "\U0001f9b4", "#8d6e63", null, "Pets")
    ];

    public record ProductSeed(string Title, string? Unit, decimal[] Quantities, params string[] Categories);

    // Correlated tuples: a product always travels with a sensible unit, quantity range and its categories
    public static readonly ProductSeed[] Products =
    [
        new("Apples", "kg", [1, 1.5m, 2], "Fruit"),
        new("Bananas", "pcs", [4, 6, 8], "Fruit"),
        new("Oranges", "kg", [1, 2], "Fruit"),
        new("Strawberries", "box", [1, 2], "Fruit"),
        new("Blueberries", "box", [1], "Fruit", "Breakfast"),
        new("Lemons", "pcs", [2, 3, 4], "Fruit"),
        new("Grapes", "kg", [0.5m, 1], "Fruit"),
        new("Avocados", "pcs", [2, 3], "Fruit", "Vegetables"),
        new("Pears", "kg", [1], "Fruit"),
        new("Kiwis", "pcs", [4, 6], "Fruit"),
        new("Tomatoes", "kg", [0.5m, 1], "Vegetables"),
        new("Cucumber", "pcs", [1, 2], "Vegetables"),
        new("Carrots", "kg", [1], "Vegetables"),
        new("Onions", "kg", [1, 2], "Vegetables"),
        new("Garlic", "pcs", [1, 2], "Vegetables", "Spices"),
        new("Potatoes", "kg", [2.5m, 5], "Vegetables"),
        new("Broccoli", "pcs", [1, 2], "Vegetables"),
        new("Spinach", "bag", [1], "Vegetables"),
        new("Bell peppers", "pcs", [2, 3], "Vegetables"),
        new("Lettuce", "pcs", [1], "Vegetables"),
        new("Mushrooms", "g", [250, 500], "Vegetables"),
        new("Zucchini", "pcs", [1, 2], "Vegetables"),
        new("Basil", "pot", [1], "Herbs"),
        new("Parsley", "bunch", [1], "Herbs"),
        new("Fresh mint", "bunch", [1], "Herbs"),
        new("Semi-skimmed milk", "l", [1, 2, 6], "Milk"),
        new("Oat milk", "l", [1, 2], "Milk", "Breakfast"),
        new("Whole milk", "l", [1, 2], "Milk"),
        new("Gouda", "g", [250, 500], "Cheese"),
        new("Parmesan", "g", [150, 200], "Cheese", "Pasta & Rice"),
        new("Brie", "pcs", [1], "Cheese"),
        new("Mozzarella", "pcs", [1, 2], "Cheese"),
        new("Feta", "pcs", [1], "Cheese"),
        new("Greek yogurt", "pot", [1, 2], "Yogurt"),
        new("Fruit yogurt", "pack", [1, 2], "Yogurt"),
        new("Eggs", "pcs", [6, 12], "Eggs"),
        new("Butter", "pack", [1, 2], "Butter & Spreads"),
        new("Peanut butter", "jar", [1], "Butter & Spreads", "Nuts"),
        new("Jam", "jar", [1], "Butter & Spreads"),
        new("Sourdough bread", "pcs", [1], "Bread"),
        new("Whole wheat bread", "pcs", [1, 2], "Bread"),
        new("Baguette", "pcs", [1, 2], "Bread"),
        new("Croissants", "pcs", [4, 6], "Pastry"),
        new("Pain au chocolat", "pcs", [2, 4], "Pastry", "Chocolate"),
        new("Minced beef", "g", [500, 1000], "Meat"),
        new("Steak", "pcs", [2, 4], "Meat"),
        new("Pork chops", "pcs", [2, 4], "Meat"),
        new("Chicken breast", "g", [400, 800], "Poultry"),
        new("Chicken thighs", "kg", [1], "Poultry"),
        new("Salmon fillet", "pcs", [2, 4], "Fish & Seafood"),
        new("Shrimps", "g", [200, 400], "Fish & Seafood"),
        new("Cod", "g", [400], "Fish & Seafood"),
        new("Ham", "g", [150, 200], "Cold Cuts"),
        new("Salami", "g", [100, 150], "Cold Cuts"),
        new("Bacon", "pack", [1, 2], "Cold Cuts", "Meat"),
        new("Spaghetti", "pack", [1, 2], "Pasta & Rice"),
        new("Penne", "pack", [1, 2], "Pasta & Rice"),
        new("Basmati rice", "kg", [1, 2], "Pasta & Rice"),
        new("Canned tomatoes", "can", [2, 4], "Canned Goods", "Sauces & Condiments"),
        new("Chickpeas", "can", [1, 2], "Canned Goods"),
        new("Tuna", "can", [2, 3], "Canned Goods", "Fish & Seafood"),
        new("Coconut milk", "can", [1, 2], "Canned Goods"),
        new("Ketchup", "bottle", [1], "Sauces & Condiments"),
        new("Mayonnaise", "jar", [1], "Sauces & Condiments"),
        new("Olive oil", "bottle", [1], "Sauces & Condiments"),
        new("Soy sauce", "bottle", [1], "Sauces & Condiments"),
        new("Pesto", "jar", [1, 2], "Sauces & Condiments", "Herbs"),
        new("Black pepper", "jar", [1], "Spices"),
        new("Sea salt", "pack", [1], "Spices"),
        new("Paprika powder", "jar", [1], "Spices"),
        new("Cinnamon", "jar", [1], "Spices", "Baking"),
        new("Muesli", "pack", [1], "Cereals"),
        new("Oatmeal", "pack", [1], "Cereals"),
        new("Cornflakes", "box", [1], "Cereals"),
        new("Flour", "kg", [1], "Baking"),
        new("Sugar", "kg", [1], "Baking"),
        new("Baking powder", "pack", [1], "Baking"),
        new("Frozen peas", "bag", [1], "Frozen Vegetables"),
        new("Frozen spinach", "bag", [1], "Frozen Vegetables"),
        new("Vanilla ice cream", "tub", [1], "Ice Cream"),
        new("Ice lollies", "box", [1], "Ice Cream"),
        new("Frozen pizza", "pcs", [1, 2, 3], "Frozen Meals"),
        new("Lasagne", "pcs", [1], "Frozen Meals"),
        new("Sparkling water", "bottle", [6, 12], "Water"),
        new("Still water", "bottle", [6], "Water"),
        new("Cola", "bottle", [1, 2, 6], "Soft Drinks"),
        new("Lemonade", "bottle", [1, 2], "Soft Drinks"),
        new("Orange juice", "l", [1, 2], "Juice"),
        new("Apple juice", "l", [1, 2], "Juice"),
        new("Ground coffee", "pack", [1, 2], "Coffee & Tea"),
        new("Coffee beans", "kg", [1], "Coffee & Tea"),
        new("Green tea", "box", [1], "Coffee & Tea"),
        new("Black tea", "box", [1], "Coffee & Tea"),
        new("Pilsner", "crate", [1], "Beer & Wine"),
        new("Red wine", "bottle", [1, 2], "Beer & Wine"),
        new("White wine", "bottle", [1, 2], "Beer & Wine"),
        new("Paprika chips", "bag", [1, 2], "Chips"),
        new("Salted chips", "bag", [1, 2], "Chips"),
        new("Dark chocolate", "bar", [1, 2], "Chocolate"),
        new("Milk chocolate", "bar", [1, 2, 3], "Chocolate"),
        new("Butter cookies", "pack", [1], "Cookies"),
        new("Speculoos", "pack", [1, 2], "Cookies", "Breakfast"),
        new("Cashews", "bag", [1], "Nuts"),
        new("Mixed nuts", "bag", [1], "Nuts"),
        new("Dish soap", "bottle", [1], "Cleaning"),
        new("All-purpose cleaner", "bottle", [1], "Cleaning"),
        new("Sponges", "pack", [1], "Cleaning"),
        new("Trash bags", "roll", [1, 2], "Cleaning", "Paper Goods"),
        new("Laundry detergent", "bottle", [1], "Laundry"),
        new("Fabric softener", "bottle", [1], "Laundry"),
        new("Toilet paper", "pack", [1, 2], "Paper Goods"),
        new("Kitchen roll", "pack", [1], "Paper Goods"),
        new("Tissues", "box", [1, 2, 3], "Paper Goods", "Hygiene"),
        new("Toothpaste", "tube", [1, 2], "Oral Care"),
        new("Toothbrushes", "pack", [1], "Oral Care"),
        new("Dental floss", "pcs", [1], "Oral Care"),
        new("Shampoo", "bottle", [1], "Hair Care"),
        new("Conditioner", "bottle", [1], "Hair Care"),
        new("Shower gel", "bottle", [1, 2], "Hygiene"),
        new("Deodorant", "pcs", [1], "Hygiene"),
        new("Hand soap", "bottle", [1], "Hygiene", "Cleaning"),
        new("Diapers size 4", "pack", [1, 2], "Diapers"),
        new("Baby wipes", "pack", [1, 2, 3], "Diapers", "Hygiene"),
        new("Fruit puree pouches", "pack", [1, 2], "Baby Food", "Fruit"),
        new("Baby porridge", "box", [1], "Baby Food", "Cereals"),
        new("Dry cat food", "bag", [1], "Pet Food"),
        new("Wet dog food", "can", [6, 12], "Pet Food"),
        new("Cat litter", "bag", [1], "Pets")
    ];

    public record ListSeed(string Title, string Color, string? Description, params string[] FocusCategories);

    public static readonly ListSeed[] Lists =
    [
        new("Weekly groceries", "#43a047", "The big weekly run"),
        new("Weekend market", "#fb8c00", "Saturday morning market", "Fresh", "Deli", "Bakery"),
        new("BBQ party", "#e53935", "Saturday BBQ with friends", "Meat & Fish", "Drinks", "Snacks & Sweets", "Sauces & Condiments"),
        new("Drugstore", "#ec407a", null, "Personal Care", "Household", "Baby"),
        new("Breakfast stock", "#ffb300", null, "Breakfast"),
        new("Pantry refill", "#8d6e63", "Long-life supplies", "Pantry", "Frozen"),
        new("Pet shop", "#795548", null, "Pets"),
        new("Movie night", "#8e24aa", null, "Snacks & Sweets", "Drinks", "Frozen Meals"),
        new("Cleaning day", "#607d8b", null, "Household"),
        new("Pasta night", "#ff7043", "Italian dinner", "Pasta & Rice", "Cheese", "Vegetables", "Herbs", "Beer & Wine"),
        new("Baby essentials", "#ffb74d", null, "Baby"),
        new("Healthy week", "#2e7d32", "Meal prep", "Fruit", "Vegetables", "Poultry", "Fish & Seafood", "Yogurt")
    ];

    // Notes are correlated with the product: generic ones fit anything, the others only their category branch
    public static readonly string[] GenericNotes = ["store brand is fine", "check for offers", "the big pack", "family size"];

    public static readonly Dictionary<string, string[]> NotesByRoot = new()
    {
        ["Fresh"] = ["organic if possible", "only if ripe", "loose, not pre-packed"],
        ["Dairy & Eggs"] = ["lactose-free", "check the expiry date", "organic if possible"],
        ["Meat & Fish"] = ["ask at the counter", "free-range", "check the expiry date"],
        ["Deli"] = ["ask at the counter", "thinly sliced"],
        ["Bakery"] = ["freshly baked", "sliced please"],
        ["Snacks & Sweets"] = ["for the kids' lunchboxes", "for movie night"],
        ["Household"] = ["refill pack", "the unscented one"],
        ["Personal Care"] = ["refill pack", "sensitive skin"],
        ["Baby"] = ["the same brand as last time"],
        ["Pets"] = ["the same brand as last time", "grain-free"],
        ["Drinks"] = ["chilled", "the big bottles"],
        ["Breakfast"] = ["for Sunday brunch"]
    };

    /// <summary>The root categories a category belongs to (any parent path).</summary>
    public static IEnumerable<string> RootsOf(string category)
    {
        var seed = Categories.First(c => c.Title == category);
        return seed.Parents.Length == 0 ? [category] : seed.Parents.SelectMany(RootsOf).Distinct();
    }

    public static string[] NotesFor(ProductSeed product)
        => GenericNotes
            .Concat(product.Categories.SelectMany(RootsOf).Distinct().SelectMany(r => NotesByRoot.GetValueOrDefault(r, [])))
            .Distinct()
            .ToArray();
}
