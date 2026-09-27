namespace EventPlanner.Api.Data.Seeding;

/// <summary>Correlated reference tuples for the seeder, so generated rows make sense together.</summary>
internal static class SeedCatalog
{
    public record CategorySeed(string Title, string Color, string Icon, string Description, int MinDays, int MaxDays,
        int MinSize, int MaxSize, string[] TitleTemplates, string[] Topics, string[] SessionTemplates, int MaxSpeakers);

    public static readonly CategorySeed[] Categories =
    [
        new("Conference", "#7c3aed", "mic", "Multi-day flagship gatherings with keynotes and parallel tracks.", 2, 3, 150, 600,
            ["{0} Summit {1}", "{0} Conference {1}", "{0} Days {1}", "The Future of {0}"],
            ["Cloud", "AI", "Data", "Security", "Sustainability", "Customer Experience", "Digital Health", "FinTech"],
            ["Keynote: The State of {0}", "{0} at Scale", "Panel: Where {0} Goes Next", "Case Study: {0} in Production", "Lessons Learned in {0}", "Closing Keynote: {0} Beyond {1}"], 3),
        new("Workshop", "#f97316", "tools", "Hands-on sessions in small groups - bring your laptop.", 1, 1, 12, 40,
            ["{0} Hands-on Workshop", "{0} Bootcamp", "Deep Dive: {0}", "Build It: {0}"],
            ["Kubernetes", "Vue 3", "Power BI", "Prompt Engineering", "Figma", "Terraform", ".NET 10", "SQL Tuning"],
            ["Setting Up {0}", "Lab 1: {0} Basics", "Lab 2: Advanced {0}", "Q&A and Wrap-up"], 2),
        new("Tech Talk", "#0ea5e9", "lightning-charge", "Short lunchtime talks on the tech we use every day.", 1, 1, 30, 120,
            ["Lunch & Learn: {0}", "Tech Talk: {0}", "{0} in 45 Minutes"],
            ["GraphQL", "Observability", "Rust", "Zero Trust", "Event Sourcing", "WebAssembly", "LLM Agents", "Edge Computing"],
            ["{0}: An Introduction", "{0} Live Demo", "Ask Me Anything: {0}"], 1),
        new("Team Building", "#22c55e", "people", "Get to know your colleagues outside the office.", 1, 2, 20, 120,
            ["{0} Challenge", "Team Day: {0}", "{0} Adventure"],
            ["Escape Room", "Cooking", "Kayaking", "Treasure Hunt", "Improv Theatre", "Mountain Biking", "Orienteering"],
            ["Welcome and Team Split", "{0}: Round 1", "{0}: Final", "Dinner and Awards"], 1),
        new("Training", "#eab308", "mortarboard", "Certified courses and soft-skill training.", 1, 3, 10, 30,
            ["{0} Training", "{0} Certification Course", "Mastering {0}"],
            ["Leadership", "Negotiation", "Time Management", "First Aid", "GDPR", "Agile Coaching", "Public Speaking", "Scrum"],
            ["Module 1: {0} Fundamentals", "Module 2: Applying {0}", "Module 3: {0} Practice", "Exam Preparation", "Assessment"], 1),
        new("Hackathon", "#ec4899", "code-slash", "Build something amazing in 24 to 48 hours.", 2, 2, 30, 150,
            ["{0} Hackathon {1}", "Hack the {0}", "{0} Jam"],
            ["Green IT", "Accessibility", "Open Data", "Smart Office", "Customer Bots", "Internal Tools"],
            ["Kick-off and Pitches", "Team Formation", "Hacking Sprint", "Mentor Round", "Demo Time", "Jury and Awards"], 2),
        new("Networking", "#14b8a6", "chat-dots", "Meet peers, partners and customers.", 1, 1, 40, 250,
            ["{0} Meetup", "{0} Networking Night", "{0} Mixer"],
            ["Women in Tech", "Partners", "Start-up", "Alumni", "Product Community", "Developer Community"],
            ["Welcome Drinks", "Lightning Talks: {0}", "Speed Networking", "Walking Dinner"], 2),
        new("Wellness", "#84cc16", "heart-pulse", "Take care of body and mind.", 1, 1, 10, 60,
            ["{0} Day", "Mindful {0}", "{0} Retreat"],
            ["Yoga", "Mindfulness", "Nutrition", "Sleep", "Stress Management", "Running"],
            ["Introduction to {0}", "{0} Practice", "Reflection and Tea"], 1),
        new("Town Hall", "#6366f1", "megaphone", "Company updates straight from leadership.", 1, 1, 100, 800,
            ["Town Hall {1}: {0}", "All Hands: {0}", "Quarterly Update: {0}"],
            ["Q1 Results", "Q2 Results", "Q3 Results", "Strategy 2027", "New Organisation", "Year in Review"],
            ["Opening", "{0}", "Department Highlights", "Open Q&A"], 3),
        new("Product Launch", "#ef4444", "rocket-takeoff", "Be the first to see what is shipping next.", 1, 1, 50, 400,
            ["Launch: {0}", "Introducing {0}", "{0} Release Party"],
            ["Nova Platform", "Pulse Analytics", "Atlas Mobile", "Orbit CRM", "Helix API", "Quantum Search"],
            ["Product Keynote: {0}", "Live Demo: {0}", "Roadmap and Q&A", "Celebration Drinks"], 2),
    ];

    public record LocationSeed(string Title, string Address, string City, string Country, int Capacity, string Description);

    public static readonly LocationSeed[] Locations =
    [
        new("HQ Auditorium", "Rue de la Loi 120", "Brussels", "Belgium", 350, "Our main auditorium with stage, 3 screens and live streaming."),
        new("HQ Workshop Rooms", "Rue de la Loi 120", "Brussels", "Belgium", 40, "Four combinable workshop rooms on the 5th floor."),
        new("Tour & Taxis", "Havenlaan 86C", "Brussels", "Belgium", 1200, "Iconic industrial venue for large conferences."),
        new("Antwerp Harbour House", "Zaha Hadidplein 1", "Antwerp", "Belgium", 220, "Waterfront venue with a panoramic terrace."),
        new("Ghent ICC", "Van Rysselberghedreef 2", "Ghent", "Belgium", 900, "International congress centre in the Citadel Park."),
        new("Leuven Innovation Hub", "Kapeldreef 75", "Leuven", "Belgium", 180, "Tech campus with maker space and labs."),
        new("Bruges Meeting & Convention Centre", "Beursplein 1", "Bruges", "Belgium", 600, "Modern venue in the historic city centre."),
        new("Liege Cite Miroir", "Place Xavier Neujean 22", "Liege", "Belgium", 450, "Former swimming pool turned cultural venue."),
        new("Amsterdam Tech Loft", "Keizersgracht 390", "Amsterdam", "Netherlands", 160, "Canal-side loft with rooftop bar."),
        new("Rotterdam Ahoy", "Ahoyweg 10", "Rotterdam", "Netherlands", 2500, "Large conference and exhibition centre."),
        new("Utrecht Jaarbeurs Studio", "Jaarbeursplein 6", "Utrecht", "Netherlands", 300, "Flexible studio next to the central station."),
        new("Paris Station F", "5 Parvis Alan Turing", "Paris", "France", 1000, "The world's biggest start-up campus."),
        new("Lille Euratechnologies", "165 Avenue de Bretagne", "Lille", "France", 250, "Digital innovation hub in a former textile mill."),
        new("Luxembourg Kirchberg Forum", "4 Place de l'Europe", "Luxembourg", "Luxembourg", 400, "Conference centre in the European district."),
        new("Cologne Loft Hall", "Mathias-Bruggen-Strasse 55", "Cologne", "Germany", 280, "Former factory hall with exposed brick."),
        new("Ardennes Retreat Lodge", "Route de la Foret 12", "Durbuy", "Belgium", 60, "Countryside lodge for retreats and team days."),
        new("Coast Beach Pavilion", "Zeedijk 300", "Ostend", "Belgium", 120, "Seaside pavilion with outdoor terrace."),
        new("Mechelen Lamot", "Van Beethovenstraat 8", "Mechelen", "Belgium", 500, "Congress centre in a former brewery."),
        new("Online (Teams)", "-", "Remote", "-", 1000, "Virtual event streamed via Microsoft Teams."),
        new("Hasselt Kinepolis Business", "Via Media 1", "Hasselt", "Belgium", 700, "Cinema halls with giant screens for keynotes."),
    ];

    public static readonly (string Department, string[] Titles)[] Departments =
    [
        ("Engineering", ["Software Engineer", "Senior Developer", "Tech Lead", "DevOps Engineer", "QA Engineer"]),
        ("Product", ["Product Manager", "Product Owner", "UX Designer", "UX Researcher"]),
        ("Sales", ["Account Executive", "Sales Manager", "Business Developer"]),
        ("Marketing", ["Marketing Specialist", "Content Writer", "Brand Manager", "Event Manager"]),
        ("Finance", ["Accountant", "Financial Controller", "Payroll Officer"]),
        ("HR", ["HR Business Partner", "Recruiter", "Learning & Development Lead"]),
        ("Operations", ["Operations Manager", "Office Manager", "Facility Coordinator"]),
        ("Customer Success", ["Customer Success Manager", "Support Engineer", "Implementation Consultant"]),
    ];

    public static readonly string[] SpeakerTopics =
    [
        "AI & Machine Learning", "Cloud Architecture", "Cybersecurity", "Leadership", "Product Management", "UX Design",
        "Data Engineering", "DevOps", "Sustainability", "Wellbeing", "Agile", "Sales Strategy", "Mobile Development", "Web Performance"
    ];

    public static readonly string[] Rooms = ["Main Stage", "Room A", "Room B", "Room C", "Lab 1", "Lab 2", "Foyer", "Studio"];
    public static readonly string[] Tracks = ["Main", "Tech", "Business", "Hands-on"];
}
