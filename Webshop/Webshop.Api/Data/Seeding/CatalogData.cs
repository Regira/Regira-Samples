namespace Webshop.Api.Data.Seeding;

/// <summary>
/// Correlated reference data for the seeder: each category carries its own brands and product types,
/// and each product type carries its own price range and features (drawn together as a tuple).
/// </summary>
public static class CatalogData
{
    public record ProductType(string Name, decimal MinPrice, decimal MaxPrice, string[] Features);

    public record CategorySeed(
        string Title, string Slug, string Icon, string Color, string Description,
        (string Name, string Country)[] Brands, ProductType[] Types);

    public static readonly CategorySeed[] Categories =
    [
        new("Audio", "audio", "headphones", "#6366f1", "Headphones, speakers and everything that sounds great.",
            [("Sonora", "Denmark"), ("Auralis", "Germany"), ("Bassline", "the United Kingdom"), ("Echowave", "Japan")],
            [
                new("Wireless Headphones", 59, 349, ["active noise cancelling", "up to 30 hours of battery life", "plush memory-foam ear cushions", "multipoint Bluetooth 5.3", "a foldable, travel-ready design"]),
                new("True Wireless Earbuds", 39, 249, ["adaptive noise cancelling", "an IPX5 sweat-resistant build", "a pocketable wireless charging case", "clear six-mic calls", "touch controls"]),
                new("Bluetooth Speaker", 29, 299, ["360-degree sound", "an IP67 waterproof housing", "a 20-hour battery", "stereo pairing", "a built-in carry strap"]),
                new("Soundbar", 149, 799, ["Dolby Atmos support", "a wireless subwoofer", "HDMI eARC", "clear dialogue enhancement", "Wi-Fi multiroom streaming"]),
                new("Turntable", 129, 599, ["a belt-driven platter", "a built-in phono preamp", "a pre-mounted cartridge", "USB recording", "an anti-resonance plinth"]),
                new("Studio Monitor", 99, 499, ["a flat, honest frequency response", "room-correction controls", "a 5in Kevlar woofer", "balanced XLR inputs", "a front-ported cabinet"])
            ]),
        new("Computers", "computers", "laptop", "#0ea5e9", "Laptops, monitors and accessories for work and play.",
            [("Nimbus", "the United States"), ("Vertex", "Taiwan"), ("Quantix", "South Korea"), ("Corelink", "the Netherlands")],
            [
                new("Ultrabook 14in", 799, 1899, ["an all-day battery", "a 2.8K OLED display", "a precision aluminium chassis", "Thunderbolt 4 ports", "a backlit keyboard"]),
                new("Gaming Laptop 16in", 1199, 2799, ["a 240Hz display", "a dedicated RTX-class GPU", "vapour-chamber cooling", "per-key RGB lighting", "32GB of fast RAM"]),
                new("27in 4K Monitor", 249, 799, ["a factory-calibrated IPS panel", "USB-C with 90W charging", "a height-adjustable stand", "HDR400 support", "slim three-sided bezels"]),
                new("Mechanical Keyboard", 59, 219, ["hot-swappable switches", "PBT double-shot keycaps", "a gasket-mounted plate", "tri-mode wireless", "a compact 75% layout"]),
                new("Wireless Mouse", 19, 129, ["an 8K DPI optical sensor", "silent clicks", "a 70-day battery", "an ergonomic sculpted shape", "multi-device switching"]),
                new("USB-C Dock", 79, 299, ["dual 4K display output", "100W pass-through charging", "2.5Gb Ethernet", "an SD card reader", "a single-cable setup"]),
                new("Portable SSD 1TB", 79, 199, ["read speeds up to 2000MB/s", "a drop-resistant shell", "hardware encryption", "a pocket-sized design", "USB-C and USB-A cables"])
            ]),
        new("Phones & Tablets", "phones-tablets", "phone", "#14b8a6", "Smartphones, tablets, wearables and the gear that keeps them going.",
            [("Lumio", "Finland"), ("Orbitel", "Sweden"), ("Zentra", "Singapore")],
            [
                new("Smartphone 128GB", 299, 899, ["a 120Hz OLED display", "a dual-lens camera", "all-day battery life", "five years of software updates", "IP68 water resistance"]),
                new("Smartphone Pro 256GB", 799, 1399, ["a triple-lens camera with 5x zoom", "a titanium frame", "wireless charging", "8K video recording", "a 1-120Hz adaptive display"]),
                new("Tablet 11in", 299, 999, ["a laminated 2.4K display", "stylus support", "quad speakers", "a keyboard-cover connector", "a 12-hour battery"]),
                new("Smartwatch", 129, 499, ["all-day health tracking", "built-in GPS", "an always-on AMOLED screen", "a 7-day battery", "contactless payments"]),
                new("Fast Charger 65W", 19, 69, ["GaN technology", "two USB-C ports", "a foldable plug", "laptop-grade output", "smart power sharing"]),
                new("Protective Case", 12, 49, ["shock-absorbing corners", "a MagSafe-compatible magnet ring", "a raised camera lip", "a slim matte finish", "recycled materials"]),
                new("Power Bank 20000mAh", 25, 79, ["45W USB-C fast charging", "charging three devices at once", "a battery level display", "an airline-safe capacity", "pass-through charging"])
            ]),
        new("Smart Home", "smart-home", "house-gear", "#f59e0b", "Connected devices that make your home safer and smarter.",
            [("Hearthly", "the Netherlands"), ("Brightnest", "Belgium"), ("Veyra", "France")],
            [
                new("Smart Bulb Starter Kit", 29, 129, ["16 million colours", "Matter support", "scheduled wake-up light", "a bridge-free setup", "energy-saving LEDs"]),
                new("Video Doorbell", 79, 249, ["2K HDR video", "a head-to-toe field of view", "two-way talk", "package detection", "end-to-end encrypted recordings"]),
                new("Smart Thermostat", 99, 279, ["learning schedules", "room-by-room control", "energy usage insights", "geofencing", "a crisp round display"]),
                new("Robot Vacuum", 199, 899, ["LiDAR navigation", "a self-emptying base", "mopping and vacuuming in one pass", "no-go zones in the app", "5000Pa suction"]),
                new("Indoor Security Camera", 39, 179, ["1080p night vision", "a privacy shutter", "person detection", "local microSD storage", "a 360-degree pan and tilt"]),
                new("Smart Plug 4-Pack", 25, 59, ["energy monitoring", "voice assistant control", "away mode", "a compact design", "local automations"]),
                new("Air Purifier", 129, 499, ["a HEPA H13 filter", "a real-time air quality sensor", "a whisper-quiet sleep mode", "coverage up to 60m2", "filter-life tracking"])
            ]),
        new("Kitchen", "kitchen", "cup-hot", "#ef4444", "Tools and appliances for everyday cooking and great coffee.",
            [("Cucina Viva", "Italy"), ("Brewster", "the United Kingdom"), ("Ferro & Co", "Germany")],
            [
                new("Espresso Machine", 149, 1299, ["a 15-bar pump", "a steam wand for silky milk", "PID temperature control", "a built-in conical grinder", "a 30-second heat-up"]),
                new("Air Fryer 5.5L", 69, 229, ["rapid hot-air circulation", "eight cooking presets", "a dishwasher-safe basket", "a see-through window", "up to 80% less fat"]),
                new("Chef's Knife 20cm", 39, 189, ["forged high-carbon steel", "a razor-sharp 15-degree edge", "a balanced full tang", "an olive-wood handle", "a lifetime warranty"]),
                new("Cast Iron Skillet", 29, 119, ["pre-seasoned cast iron", "even heat retention", "induction-ready base", "oven-safe up to 260C", "a helper handle"]),
                new("High-Speed Blender", 49, 399, ["a 1500W motor", "a BPA-free 2L jar", "self-cleaning mode", "stainless-steel blades", "smoothie and soup presets"]),
                new("Electric Kettle", 29, 129, ["variable temperature control", "a keep-warm function", "a brushed steel body", "a 1.7L capacity", "boil-dry protection"]),
                new("Cookware Set 8-Piece", 99, 449, ["tri-ply stainless steel", "induction-ready bases", "tempered glass lids", "stay-cool handles", "dishwasher-safe construction"])
            ]),
        new("Fashion", "fashion", "bag", "#ec4899", "Everyday wardrobe essentials made to last.",
            [("Urbanloom", "Portugal"), ("Sable & Stone", "Spain"), ("Driftline", "Denmark")],
            [
                new("Organic Cotton Tee", 15, 45, ["certified organic cotton", "a relaxed modern fit", "a pre-shrunk fabric", "reinforced shoulder seams", "a soft brushed finish"]),
                new("Slim Fit Chinos", 39, 99, ["a touch of stretch", "a tapered leg", "garment-dyed cotton twill", "deep pockets", "a timeless design"]),
                new("Merino Sweater", 59, 149, ["extra-fine merino wool", "natural temperature regulation", "a ribbed crew neck", "an itch-free knit", "a slim silhouette"]),
                new("Leather Sneakers", 69, 189, ["full-grain leather uppers", "a cushioned footbed", "a stitched rubber cupsole", "waxed cotton laces", "a clean minimalist look"]),
                new("Waterproof Rain Jacket", 79, 249, ["fully taped seams", "a 10k waterproof membrane", "a packable hood", "underarm vents", "recycled nylon shell"]),
                new("Canvas Backpack", 39, 129, ["a padded 15in laptop sleeve", "water-repellent waxed canvas", "leather trims", "a hidden back pocket", "reinforced stitching"]),
                new("Leather Belt", 25, 79, ["vegetable-tanned leather", "a solid brass buckle", "hand-finished edges", "a 35mm width", "a classic design"])
            ]),
        new("Sports & Outdoors", "sports-outdoors", "bicycle", "#22c55e", "Gear for running, training and adventures outside.",
            [("Trailforge", "Switzerland"), ("Peakline", "Austria"), ("Kinetica", "Norway")],
            [
                new("Trail Running Shoes", 79, 179, ["an aggressive lugged outsole", "a rock plate", "responsive foam cushioning", "a quick-lace system", "a breathable mesh upper"]),
                new("Yoga Mat", 19, 89, ["a non-slip natural rubber surface", "6mm joint-friendly cushioning", "alignment markings", "a carry strap", "a sweat-resistant top layer"]),
                new("Adjustable Dumbbells", 99, 399, ["weights from 2 to 24kg", "a quick-dial selector", "a compact storage tray", "a knurled steel handle", "replacing 15 pairs of dumbbells"]),
                new("Camping Tent 2P", 119, 449, ["a 3000mm waterproof rating", "an ultralight 1.6kg build", "two doors and vestibules", "colour-coded poles", "a 5-minute pitch"]),
                new("Insulated Water Bottle", 15, 45, ["double-wall vacuum insulation", "24 hours cold, 12 hours hot", "a leak-proof lid", "a powder-coated grip", "BPA-free materials"]),
                new("Cycling Helmet", 49, 199, ["MIPS impact protection", "18 cooling vents", "a magnetic buckle", "an integrated rear light", "a dial-fit system"]),
                new("Hiking Backpack 40L", 69, 229, ["a ventilated back panel", "an integrated rain cover", "hip-belt pockets", "a hydration sleeve", "adjustable torso length"])
            ]),
        new("Home & Living", "home-living", "lamp", "#a855f7", "Textiles, lighting and decor for a cozy home.",
            [("Lumenhaus", "Germany"), ("Oakhollow", "Belgium"), ("Softnest", "Sweden")],
            [
                new("Linen Duvet Cover", 69, 199, ["stonewashed European linen", "hidden button closures", "a breathable weave", "OEKO-TEX certification", "a relaxed lived-in look"]),
                new("Floor Lamp", 59, 299, ["a dimmable warm-white glow", "a linen shade", "a weighted marble base", "a foot switch", "an adjustable arm"]),
                new("Scented Candle Set", 19, 59, ["hand-poured soy wax", "cotton wicks", "a 40-hour burn time", "notes of fig and cedar", "reusable glass jars"]),
                new("Ceramic Vase", 19, 89, ["a handmade reactive glaze", "a watertight finish", "a sculptural silhouette", "a stable wide base", "unique variations in every piece"]),
                new("Wool Throw Blanket", 49, 159, ["soft lambswool", "a classic herringbone weave", "fringed edges", "a generous 130x170cm size", "natural warmth without weight"]),
                new("Oak Bookshelf", 89, 399, ["solid FSC-certified oak", "five adjustable shelves", "a natural oil finish", "a wall anchor kit", "a minimalist Scandinavian look"]),
                new("Cotton Towel Set", 29, 89, ["extra-soft 600gsm cotton", "quick-dry loops", "double-stitched hems", "OEKO-TEX certified fabric", "a hanging loop"])
            ]),
        new("Beauty & Care", "beauty-care", "droplet", "#f43f5e", "Skincare, haircare and personal care favourites.",
            [("Aurelle", "France"), ("Purebloom", "the Netherlands"), ("Vitalis", "Italy")],
            [
                new("Hydrating Face Serum", 19, 89, ["hyaluronic acid", "niacinamide", "a lightweight non-greasy formula", "fragrance-free care for sensitive skin", "vegan ingredients"]),
                new("Daily SPF 50 Sunscreen", 12, 39, ["broad-spectrum UVA/UVB protection", "an invisible finish", "water resistance", "a reef-friendly formula", "no white cast"]),
                new("Ionic Hair Dryer", 49, 399, ["ionic frizz control", "three heat settings", "a cool-shot button", "a concentrator nozzle", "a lightweight 400g body"]),
                new("Electric Toothbrush", 39, 229, ["a pressure sensor", "a 2-minute smart timer", "five cleaning modes", "a 30-day battery", "a travel case"]),
                new("Eau de Parfum 50ml", 49, 149, ["notes of bergamot and cedar", "a long-lasting sillage", "a refillable bottle", "a unisex composition", "responsibly sourced ingredients"]),
                new("Beard Trimmer", 29, 119, ["20 precision length settings", "self-sharpening steel blades", "a waterproof body", "a 90-minute runtime", "USB-C charging"])
            ]),
        new("Gaming", "gaming", "controller", "#8b5cf6", "Consoles, accessories and games for every kind of player.",
            [("Pixelforge", "Canada"), ("Arcadia", "the United States"), ("Nexplay", "Poland")],
            [
                new("Wireless Controller", 39, 89, ["hall-effect sticks", "low-latency 2.4GHz wireless", "remappable back paddles", "a 40-hour battery", "textured grips"]),
                new("Gaming Headset", 49, 249, ["7.1 surround sound", "a detachable noise-cancelling mic", "memory-foam ear cushions", "a 50mm driver", "cross-platform compatibility"]),
                new("Ergonomic Gaming Chair", 179, 499, ["adjustable 4D armrests", "adaptive lumbar support", "breathable fabric", "a 165-degree recline", "a steel frame"]),
                new("Strategy Board Game", 19, 69, ["room for two to five players", "60-minute sessions", "beautifully illustrated components", "high replay value", "a quick-start rulebook"]),
                new("RGB Gaming Keyboard", 79, 229, ["per-key RGB lighting", "optical switches", "a magnetic wrist rest", "onboard macro memory", "a dedicated media dial"]),
                new("Handheld Console", 199, 549, ["a 7in OLED screen", "a docking mode for the TV", "a 10-hour battery", "a microSD slot", "detachable controllers"]),
                new("Streaming Microphone", 59, 199, ["a cardioid condenser capsule", "a tap-to-mute sensor", "zero-latency monitoring", "a built-in pop filter", "USB-C plug-and-play"])
            ])
    ];

    public static readonly string[] Series =
        ["One", "Pro", "Air", "Max", "Lite", "Plus", "Studio", "Go", "Ultra", "Neo", "Edge", "Flex", "Prime", "Core", "Nova", "Aero"];

    public static readonly (string Country, string[] Cities, int PostalDigits, string PhoneFormat)[] Destinations =
    [
        ("Belgium", ["Brussels", "Antwerp", "Ghent", "Leuven", "Bruges", "Liege", "Namur", "Mechelen", "Hasselt"], 4, "+32 4## ## ## ##"),
        ("Netherlands", ["Amsterdam", "Rotterdam", "Utrecht", "Eindhoven", "The Hague", "Groningen", "Maastricht"], 4, "+31 6 ########"),
        ("Germany", ["Berlin", "Hamburg", "Munich", "Cologne", "Frankfurt", "Dusseldorf", "Leipzig"], 5, "+49 15# #######"),
        ("France", ["Paris", "Lyon", "Lille", "Bordeaux", "Nantes", "Strasbourg", "Toulouse"], 5, "+33 6 ## ## ## ##"),
        ("Luxembourg", ["Luxembourg", "Esch-sur-Alzette", "Differdange"], 4, "+352 691 ### ###")
    ];
}
