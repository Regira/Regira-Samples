using QCredits.Api.Entities.CreditRequests;

namespace QCredits.Api.Data.Seeding;

/// <summary>
/// Coherent seed tuples: a training topic with the purchases/activities that belong to it,
/// so titles, item descriptions, providers, credits and costs always match each other.
/// </summary>
public static class SeedCatalog
{
    public record Department(string Title, string Code, string Description, string[] JobTitles, int Weight);

    public static readonly Department[] Departments =
    [
        new("Engineering", "ENG", "Software development, architecture and DevOps.",
            ["Software Engineer", "Senior Software Engineer", "Tech Lead", "DevOps Engineer", "QA Engineer", "Solution Architect"], 30),
        new("Sales", "SAL", "Account management and new business.",
            ["Account Manager", "Sales Executive", "Pre-sales Consultant", "Sales Manager"], 12),
        new("Marketing", "MKT", "Brand, content and demand generation.",
            ["Content Marketer", "Marketing Specialist", "Brand Manager", "SEO Specialist"], 8),
        new("Finance", "FIN", "Accounting, controlling and payroll.",
            ["Accountant", "Financial Controller", "Payroll Officer", "Finance Manager"], 7),
        new("Human Resources", "HR", "Recruitment, people development and HR administration.",
            ["HR Business Partner", "Recruiter", "Learning & Development Officer", "HR Administrator"], 6),
        new("Operations", "OPS", "Service delivery, planning and facilities.",
            ["Operations Coordinator", "Service Delivery Manager", "Planner", "Facility Manager"], 10),
        new("Customer Success", "CS", "Support, onboarding and customer care.",
            ["Support Engineer", "Customer Success Manager", "Onboarding Specialist"], 11),
        new("Data & Analytics", "DATA", "BI, data engineering and data science.",
            ["Data Engineer", "Data Analyst", "BI Developer", "Data Scientist"], 6),
    ];

    /// <summary>One purchase or activity. Credits = working time (1 credit = half a day) + cost (1 credit = EUR 250).</summary>
    public record Activity(ActivityType Type, string Description, string? Provider, double TimeCredits, decimal Cost, string? Url = null)
    {
        public double Credits => TimeCredits + Math.Max(0.5, Math.Ceiling((double)Cost / 125d) / 2d) * (Cost > 0 ? 1 : 0);
    }

    public record Topic(string Title, string Motivation, string[] Departments, Activity[] Activities);

    private const string All = "*";

    public static readonly Topic[] Topics =
    [
        new("Kubernetes certification (CKA)", "Our platform team moves all services to Kubernetes; I want to be able to operate the clusters.",
            ["Engineering"],
            [
                new(ActivityType.Course, "Certified Kubernetes Administrator online course", "Linux Foundation", 0, 395m, "https://training.linuxfoundation.org"),
                new(ActivityType.Certification, "CKA exam voucher", "Linux Foundation", 1, 445m),
                new(ActivityType.SelfStudy, "Lab practice: cluster upgrades and troubleshooting", null, 4, 0m),
            ]),
        new("Azure Solutions Architect path", "Several customer projects run on Azure and I take up more architecture work.",
            ["Engineering", "Data & Analytics"],
            [
                new(ActivityType.Course, "AZ-305 Designing Microsoft Azure Infrastructure Solutions (virtual bootcamp, 2 days)", "Global Knowledge", 4, 895m),
                new(ActivityType.Certification, "AZ-305 exam", "Microsoft", 1, 165m),
                new(ActivityType.SelfStudy, "Microsoft Learn modules and practice assessments", "Microsoft Learn", 3, 0m, "https://learn.microsoft.com"),
            ]),
        new("Clean architecture & DDD", "To improve the design of the new order platform.",
            ["Engineering"],
            [
                new(ActivityType.Book, "Domain-Driven Design Distilled", "Addison-Wesley", 0, 35m),
                new(ActivityType.Book, "Implementing Domain-Driven Design", "Addison-Wesley", 0, 55m),
                new(ActivityType.SelfStudy, "Self-study: DDD reading group", null, 2, 0m),
                new(ActivityType.Course, "DDD Europe workshop day", "DDD Europe", 2, 650m),
            ]),
        new("Devoxx Belgium", "Keep up with the Java and cloud-native ecosystem and network with peers.",
            ["Engineering"],
            [
                new(ActivityType.Conference, "Devoxx Belgium conference ticket (3 days)", "Devoxx", 6, 795m, "https://devoxx.be"),
            ]),
        new("Frontend skills: Vue & TypeScript", "The team is migrating the customer portal to Vue 3.",
            ["Engineering"],
            [
                new(ActivityType.OnlineSubscription, "Vue Mastery annual subscription", "Vue Mastery", 0, 195m),
                new(ActivityType.Book, "Effective TypeScript", "O'Reilly", 0, 45m),
                new(ActivityType.SelfStudy, "Self-study: rebuild an internal tool in Vue 3", null, 4, 0m),
            ]),
        new("Test automation with Playwright", "Reduce manual regression testing on each release.",
            ["Engineering", "Customer Success"],
            [
                new(ActivityType.Course, "Playwright end-to-end testing course", "Udemy", 2, 90m),
                new(ActivityType.SelfStudy, "Self-study: automate the smoke test suite", null, 3, 0m),
            ]),
        new("Security awareness & OWASP", "Security findings in the last audit; I want to fix them structurally.",
            ["Engineering", "Operations"],
            [
                new(ActivityType.Course, "OWASP Top 10 for developers (2 days)", "SANS", 4, 700m),
                new(ActivityType.Book, "Web Application Security", "O'Reilly", 0, 50m),
            ]),
        new("Data engineering with Databricks", "We are building a lakehouse for reporting.",
            ["Data & Analytics", "Engineering"],
            [
                new(ActivityType.Course, "Databricks Data Engineer Associate learning path", "Databricks Academy", 4, 0m),
                new(ActivityType.Certification, "Databricks Certified Data Engineer Associate exam", "Databricks", 1, 190m),
            ]),
        new("Power BI advanced reporting", "Managers ask for self-service dashboards.",
            ["Data & Analytics", "Finance", "Operations"],
            [
                new(ActivityType.Course, "PL-300 Power BI Data Analyst (2 days)", "Xylos", 4, 795m),
                new(ActivityType.Book, "The Definitive Guide to DAX", "Microsoft Press", 0, 60m),
            ]),
        new("Machine learning foundations", "Explore churn prediction for Customer Success.",
            ["Data & Analytics"],
            [
                new(ActivityType.OnlineSubscription, "Coursera Plus annual subscription", "Coursera", 0, 399m, "https://www.coursera.org"),
                new(ActivityType.SelfStudy, "Self-study: Machine Learning Specialization", "Coursera", 6, 0m),
            ]),
        new("Negotiation skills", "Improve closing rates on larger deals.",
            ["Sales"],
            [
                new(ActivityType.Course, "Harvard-style negotiation workshop (2 days)", "Vlerick Business School", 4, 950m),
                new(ActivityType.Book, "Never Split the Difference", "Harper Business", 0, 25m),
            ]),
        new("Solution selling", "Move from product pitches to value conversations.",
            ["Sales", "Customer Success"],
            [
                new(ActivityType.Course, "Value-based selling training", "Sales Academy", 2, 750m),
                new(ActivityType.Book, "The Challenger Sale", "Portfolio", 0, 30m),
                new(ActivityType.OnlineSubscription, "LinkedIn Learning annual subscription", "LinkedIn", 0, 300m),
            ]),
        new("Salesforce administrator", "We are the internal owners of the CRM.",
            ["Sales", "Operations"],
            [
                new(ActivityType.SelfStudy, "Trailhead Admin trail", "Salesforce Trailhead", 4, 0m, "https://trailhead.salesforce.com"),
                new(ActivityType.Certification, "Salesforce Certified Administrator exam", "Salesforce", 1, 200m),
            ]),
        new("Content marketing & SEO", "Grow organic traffic to the website.",
            ["Marketing"],
            [
                new(ActivityType.Course, "Advanced SEO masterclass", "Semrush Academy", 2, 490m),
                new(ActivityType.OnlineSubscription, "Semrush Pro subscription (training seat)", "Semrush", 0, 250m),
                new(ActivityType.Book, "Everybody Writes", "Wiley", 0, 28m),
            ]),
        new("Marketing automation with HubSpot", "Set up nurturing flows for the new product line.",
            ["Marketing", "Sales"],
            [
                new(ActivityType.SelfStudy, "HubSpot Academy marketing automation certification", "HubSpot Academy", 3, 0m),
                new(ActivityType.Conference, "INBOUND Europe day ticket", "HubSpot", 2, 450m),
            ]),
        new("Brand storytelling", "Refresh our brand messaging for 2026.",
            ["Marketing"],
            [
                new(ActivityType.Course, "Storytelling for brands (1 day)", "Kunstmaan Academy", 2, 595m),
                new(ActivityType.Book, "Building a StoryBrand", "HarperCollins", 0, 22m),
            ]),
        new("IFRS update", "New reporting obligations for the group.",
            ["Finance"],
            [
                new(ActivityType.Course, "IFRS update seminar", "ICAB", 2, 480m),
                new(ActivityType.SelfStudy, "Self-study: IFRS 18 presentation changes", null, 2, 0m),
            ]),
        new("Excel for financial modelling", "Speed up budgeting and forecasting.",
            ["Finance", "Operations", "Sales"],
            [
                new(ActivityType.Course, "Financial modelling in Excel (2 days)", "Corporate Finance Institute", 4, 890m),
                new(ActivityType.Book, "Financial Modeling (Benninga)", "MIT Press", 0, 75m),
            ]),
        new("Belgian payroll & social legislation", "Keep payroll compliant with the latest changes.",
            ["Finance", "Human Resources"],
            [
                new(ActivityType.Course, "Payroll update seminar", "SD Worx Academy", 2, 425m),
                new(ActivityType.OnlineSubscription, "Legal news subscription (Wolters Kluwer)", "Wolters Kluwer", 0, 320m),
            ]),
        new("Coaching & feedback skills", "I coach new team members and want to do it well.",
            [All],
            [
                new(ActivityType.Course, "Coaching skills for leaders (2 days)", "Vlerick Business School", 4, 850m),
                new(ActivityType.Book, "The Coaching Habit", "Box of Crayons Press", 0, 20m),
            ]),
        new("Time management & productivity", "Handle a growing number of parallel projects.",
            [All],
            [
                new(ActivityType.Course, "Getting Things Done workshop (1 day)", "GTD Belgium", 2, 395m),
                new(ActivityType.Book, "Deep Work", "Grand Central", 0, 22m),
            ]),
        new("Project management (PRINCE2 Foundation)", "More project coordination in my role.",
            [All],
            [
                new(ActivityType.Course, "PRINCE2 Foundation training (3 days)", "PeopleCert partner", 6, 650m),
                new(ActivityType.Certification, "PRINCE2 Foundation exam", "PeopleCert", 1, 0m),
            ]),
        new("Agile & Scrum", "The department switches to Scrum.",
            [All],
            [
                new(ActivityType.Course, "Professional Scrum Master training (2 days)", "Scrum.org trainer", 4, 595m),
                new(ActivityType.Certification, "PSM I assessment", "Scrum.org", 1, 200m),
            ]),
        new("Dutch / French language course", "I work with customers in both language regions.",
            [All],
            [
                new(ActivityType.Course, "Business language course (20 lessons)", "CVO Language School", 6, 350m),
                new(ActivityType.OnlineSubscription, "Babbel annual subscription", "Babbel", 0, 90m),
            ]),
        new("Presentation skills", "I present more often to customers and management.",
            [All],
            [
                new(ActivityType.Course, "Presenting with impact (1 day)", "Kluwer Opleidingen", 2, 690m),
            ]),
        new("Recruitment & employer branding", "We need to hire 30 people this year.",
            ["Human Resources", "Marketing"],
            [
                new(ActivityType.Course, "LinkedIn recruiter masterclass", "Recruitment Academy", 2, 450m),
                new(ActivityType.Conference, "HR Tech Day ticket", "HR Square", 2, 295m),
            ]),
        new("ITIL 4 Foundation", "Align our service desk with ITIL practices.",
            ["Customer Success", "Operations"],
            [
                new(ActivityType.Course, "ITIL 4 Foundation e-learning", "Axelos partner", 3, 490m),
                new(ActivityType.Certification, "ITIL 4 Foundation exam", "PeopleCert", 1, 0m),
            ]),
        new("Customer success management", "Structure our onboarding and renewal process.",
            ["Customer Success", "Sales"],
            [
                new(ActivityType.Course, "Certified Customer Success Manager course", "SuccessHACKER", 4, 995m),
                new(ActivityType.Book, "Customer Success (Mehta)", "Wiley", 0, 30m),
            ]),
        new("Lean operations", "Reduce waste in our planning process.",
            ["Operations"],
            [
                new(ActivityType.Course, "Lean Six Sigma Yellow Belt (2 days)", "Lean Institute", 4, 850m),
                new(ActivityType.SelfStudy, "Self-study: value stream mapping of our planning", null, 2, 0m),
            ]),
        new("Online learning platform", "Continuous learning in small blocks.",
            [All],
            [
                new(ActivityType.OnlineSubscription, "O'Reilly Learning annual subscription", "O'Reilly", 0, 499m, "https://learning.oreilly.com"),
            ]),
        new("Professional reading", "Books on my current focus topics.",
            [All],
            [
                new(ActivityType.Book, "Team Topologies", "IT Revolution", 0, 30m),
                new(ActivityType.Book, "Atomic Habits", "Avery", 0, 20m),
                new(ActivityType.Book, "Crucial Conversations", "McGraw Hill", 0, 25m),
            ]),
    ];

    public static Topic[] TopicsFor(string department)
        => Topics.Where(t => t.Departments.Contains(All) || t.Departments.Contains(department)).ToArray();

    public record GroupTrainingTemplate(string Title, string Provider, string Description, double Days, decimal CostPerPerson, string? Department);

    public static readonly GroupTrainingTemplate[] GroupTrainings =
    [
        new("First aid at work", "Rode Kruis Vlaanderen", "Mandatory first-aid refresher for the prevention team.", 1, 145m, null),
        new("GDPR & information security awareness", "Internal - Legal", "Yearly awareness session for all staff.", 0.5, 40m, null),
        new("Leadership programme - module 1", "Vlerick Business School", "Leadership programme for team leads.", 2, 1100m, null),
        new("Leadership programme - module 2", "Vlerick Business School", "Follow-up module: coaching and difficult conversations.", 2, 1100m, null),
        new("Engineering hack days", "Internal - Engineering", "Two days of hands-on experimentation with new technology.", 2, 60m, "Engineering"),
        new("Secure coding workshop", "NVISO", "In-house workshop on secure coding practices.", 1, 350m, "Engineering"),
        new("Sales kick-off training", "Sales Academy", "Yearly sales methodology training at the kick-off.", 1, 290m, "Sales"),
        new("Customer empathy workshop", "Customer Care Academy", "Handling difficult customer conversations.", 1, 240m, "Customer Success"),
        new("Power BI team training", "Xylos", "Hands-on Power BI training for report builders.", 2, 520m, "Data & Analytics"),
        new("Writing for the web", "Kunstmaan Academy", "Copywriting and tone-of-voice workshop.", 1, 310m, "Marketing"),
        new("Fire safety & evacuation", "Securitas", "Training for evacuation officers.", 0.5, 85m, null),
        new("Time-management masterclass", "Kluwer Opleidingen", "Company-wide masterclass on focus and planning.", 1, 190m, null),
    ];
}
