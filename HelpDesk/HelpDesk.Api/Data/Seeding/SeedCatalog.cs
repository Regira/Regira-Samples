namespace HelpDesk.Api.Data.Seeding;

/// <summary>Hand-written, correlated seed content: every subject belongs to its category, every reply to its thread.</summary>
public static class SeedCatalog
{
    public record TeamSeed(string Title, string Color, string Email, string Description, string[] JobTitles);

    public static readonly TeamSeed[] Teams =
    [
        new("Service Desk", "#0d6efd", "servicedesk@helpdesk.test", "First line: intake, triage, accounts and general questions.",
            ["Service Desk Lead", "Service Desk Analyst", "Service Desk Analyst", "IT Support Specialist"]),
        new("Network & Connectivity", "#6f42c1", "network@helpdesk.test", "VPN, Wi-Fi, firewalls and site connectivity.",
            ["Network Engineer", "Network Engineer", "Senior Network Engineer", "Infrastructure Technician"]),
        new("Business Applications", "#198754", "apps@helpdesk.test", "Mail, collaboration suite, ERP and CRM.",
            ["Application Specialist", "ERP Consultant", "CRM Administrator", "M365 Administrator"]),
        new("Hardware & Devices", "#fd7e14", "devices@helpdesk.test", "Laptops, desktops, printers and mobile devices.",
            ["Field Technician", "Field Technician", "Device Management Specialist", "Hardware Technician"]),
        new("Billing & Licensing", "#20c997", "billing@helpdesk.test", "Invoices, subscriptions and license seats.",
            ["Billing Specialist", "Account Manager", "License Coordinator", "Billing Specialist"]),
        new("Security & Access", "#dc3545", "security@helpdesk.test", "Identity, MFA, access requests and incidents.",
            ["Security Analyst", "Identity Engineer", "Security Analyst", "SOC Analyst"])
    ];

    public record CategorySeed(string Title, string Team, string Color, string Icon, string Description, int Weight, (string Subject, string Body)[] Issues);

    public static readonly CategorySeed[] Categories =
    [
        new("Account & Login", "Security & Access", "#dc3545", "bi bi-person-lock", "Sign-in problems, locked accounts, MFA.", 10,
        [
            ("Account locked after too many sign-in attempts", "I was typing my password on my phone and now my account is locked. I cannot sign in to anything since this morning."),
            ("MFA prompt keeps looping on sign-in", "Every time I approve the MFA request on my phone, the browser asks me to sign in again. It loops endlessly."),
            ("New phone - need to move my authenticator", "I got a new phone and the authenticator app did not transfer. I cannot approve sign-ins anymore."),
            ("Cannot sign in to the customer portal", "The portal says my credentials are invalid, but the same password works for e-mail."),
            ("Access request for shared finance folder", "I joined the finance team last week and need read/write access to the shared finance folder.")
        ]),
        new("Password Reset", "Service Desk", "#0d6efd", "bi bi-key", "Forgotten or expired passwords.", 9,
        [
            ("Password expired while on holiday", "My password expired while I was away and the self-service reset does not send me the code."),
            ("Self-service password reset not working", "The reset page shows an error 'unable to verify identity' when I enter my details."),
            ("Please reset password for new colleague", "Our new colleague starts today and never received the initial password e-mail."),
            ("Forgot password for the ERP system", "I have not used the ERP for months and forgot my password. Can you reset it?")
        ]),
        new("Email & Calendar", "Business Applications", "#198754", "bi bi-envelope", "Mailbox, calendar and shared mailbox issues.", 9,
        [
            ("Emails stuck in the outbox", "Since this morning all my outgoing e-mails stay in the outbox. Incoming mail works fine."),
            ("Shared mailbox not visible in Outlook", "I was given access to the sales shared mailbox but it does not appear in Outlook."),
            ("Calendar invites arrive with the wrong time zone", "Meeting invites from our US colleagues show up one hour off in my calendar."),
            ("Mailbox almost full warning", "I keep getting a warning that my mailbox is 98% full. Can the quota be increased?"),
            ("External recipients get our mail in spam", "Several customers told us our e-mails land in their spam folder since last week.")
        ]),
        new("VPN & Remote Access", "Network & Connectivity", "#6f42c1", "bi bi-globe2", "VPN client and remote desktop access.", 8,
        [
            ("VPN disconnects every few minutes", "When working from home the VPN drops every 5 to 10 minutes, which kills my remote desktop session."),
            ("Cannot connect to VPN from hotel network", "I am travelling and the VPN client times out on the hotel Wi-Fi. Mobile hotspot works."),
            ("Remote desktop to office PC fails", "Remote desktop says the computer cannot be found, although the office PC is switched on."),
            ("VPN client asks for certificate", "After the latest update the VPN client asks me to select a certificate and none of them work.")
        ]),
        new("Wi-Fi & Network", "Network & Connectivity", "#6610f2", "bi bi-wifi", "Office Wi-Fi, cabling and network drives.", 7,
        [
            ("Wi-Fi very slow in meeting room 2", "Video calls in meeting room 2 freeze constantly. Other rooms seem fine."),
            ("Network drive not mapped after login", "The S: drive with our project files is no longer mapped when I log in."),
            ("Guest Wi-Fi password for visitors", "We have external auditors visiting next week who need guest Wi-Fi access."),
            ("No network on desk port 3.14", "The network port at desk 3.14 gives no connection; the laptop works on other desks.")
        ]),
        new("Laptop & Desktop", "Hardware & Devices", "#fd7e14", "bi bi-laptop", "Workstations, docking stations and peripherals.", 9,
        [
            ("Laptop does not charge on the docking station", "My laptop only charges with the original adapter, not through the docking station."),
            ("Blue screen after latest update", "Since last night's updates my laptop shows a blue screen during startup about half of the time."),
            ("Second monitor not detected", "My second monitor stays black after I moved desks. The cable seems fine."),
            ("Keyboard keys not responding", "Several keys on my laptop keyboard (E, R and T) stopped responding."),
            ("Request for a replacement laptop", "My laptop is five years old and very slow. Is it eligible for replacement?")
        ]),
        new("Printer & Scanner", "Hardware & Devices", "#e76f00", "bi bi-printer", "Printing, scanning and multifunction devices.", 6,
        [
            ("Printer on floor 2 shows paper jam", "The multifunction printer on the second floor keeps reporting a paper jam, but there is no paper stuck."),
            ("Scan to e-mail not arriving", "Scans from the copier used to arrive by e-mail; since Monday nothing comes through."),
            ("Cannot add the new office printer", "The new printer in the east wing does not show up when I try to add it."),
            ("Print jobs stuck in the queue", "My print jobs stay in the queue with status 'error'. Restarting did not help.")
        ]),
        new("Mobile Devices", "Hardware & Devices", "#ff922b", "bi bi-phone", "Company phones and tablets.", 5,
        [
            ("Company phone not syncing e-mail", "My company phone stopped syncing e-mail two days ago. Calendar still works."),
            ("Lost company phone", "I lost my company phone on the train this morning. Please wipe it and advise on a replacement."),
            ("Tablet enrolment fails", "Enrolling the new sales tablet in device management fails at the 'install profile' step.")
        ]),
        new("Software Installation", "Service Desk", "#0dcaf0", "bi bi-download", "Software requests and installation problems.", 7,
        [
            ("Request to install a PDF editor", "I need to edit PDF contracts regularly. Could you install a PDF editor on my laptop?"),
            ("Installation of design software fails", "The installer for the design software stops at 60% with error 1603."),
            ("Need admin rights for a one-time install", "Our supplier sent a configuration tool that needs to be installed with admin rights."),
            ("Update of statistics package", "The statistics package on our analysis PCs is outdated and cannot open newer files.")
        ]),
        new("ERP / CRM", "Business Applications", "#2f9e44", "bi bi-bar-chart", "Business applications: ERP, CRM, reporting.", 8,
        [
            ("ERP report export to Excel is empty", "When I export the monthly sales report to Excel the file only contains the header row."),
            ("CRM shows duplicate customer records", "Several customers appear twice in the CRM since the last import."),
            ("Cannot post invoices in the ERP", "Posting a purchase invoice fails with 'period is closed' although the period should be open."),
            ("Dashboard in CRM loads very slowly", "The sales dashboard takes more than a minute to load since this week."),
            ("New user needs CRM license and role", "A new account manager starts Monday and needs a CRM license with the sales role.")
        ]),
        new("Billing & Invoices", "Billing & Licensing", "#20c997", "bi bi-receipt", "Invoice questions, credit notes and payment terms.", 6,
        [
            ("Invoice amount does not match the quote", "Our last invoice is higher than the agreed quote. Could you check the line items?"),
            ("Request for a credit note", "We were billed twice for the same support hours in August. Please issue a credit note."),
            ("Change invoice address", "Our company moved; please update the invoice address for future invoices."),
            ("Copy of an invoice from last quarter", "Our accountant needs a copy of the invoice from March for the audit.")
        ]),
        new("Licenses & Subscriptions", "Billing & Licensing", "#12b886", "bi bi-patch-check", "License seats and subscription changes.", 5,
        [
            ("Add five licenses to our subscription", "We are growing and need five extra user licenses as of next month."),
            ("License shows as expired", "The application tells us our license expired, although we renewed it last week."),
            ("Downgrade subscription plan", "We would like to move from the premium plan to the standard plan at renewal.")
        ]),
        new("Security Incident", "Security & Access", "#c92a2a", "bi bi-shield-exclamation", "Phishing, malware and suspicious activity.", 5,
        [
            ("Suspicious e-mail asking for credentials", "I received an e-mail that looks like it comes from our bank asking me to confirm my password. I did not click anything."),
            ("Clicked a link in a phishing mail", "I am afraid I clicked a link in a phishing e-mail and entered my credentials. What should I do?"),
            ("Antivirus reports a threat on my laptop", "The antivirus popped up a warning about a trojan in my downloads folder."),
            ("Unknown sign-in from another country", "I got an alert about a sign-in to my account from a country I have never been to.")
        ])
    ];

    public static readonly string[] AgentFirstReplies =
    [
        "Thank you for reporting this. I am looking into it now and will get back to you shortly.",
        "Thanks for the details. Could you tell me since when exactly this happens and whether colleagues have the same problem?",
        "I have picked up your ticket. Can you send a screenshot of the error message you see?",
        "Thanks for reaching out. To help us investigate, could you confirm your device name and location?",
        "Hi, I have assigned this to myself. I will contact you within the hour to go through it together."
    ];

    public static readonly string[] CustomerFollowUps =
    [
        "It started yesterday afternoon. My colleague next to me has no issues.",
        "I attached a screenshot. It happens every time I try again.",
        "Thanks for the quick reply. I am available all afternoon if you want to call.",
        "I tried restarting as suggested but the problem is still there.",
        "Just checking in - is there any update on this?",
        "The device name is on the sticker; I will send it as soon as I am back at my desk."
    ];

    public static readonly string[] AgentProgress =
    [
        "I found the cause and I am applying a fix now. Please do not restart for the next 15 minutes.",
        "I have escalated this to the second-line team; they will continue with it today.",
        "We pushed an updated configuration to your device. Could you try again and let me know?",
        "A replacement part has been ordered and should arrive within two working days.",
        "I have made the change on our side. Can you confirm whether it works now?"
    ];

    public static readonly string[] AgentWaiting =
    [
        "Could you try once more and let us know whether the issue is gone? We will keep the ticket open until then.",
        "We need your approval before we proceed. Please reply to confirm.",
        "Could you let us know a time slot this week when we can take over your screen?"
    ];

    public static readonly string[] AgentResolutions =
    [
        "The issue has been resolved: the configuration was corrected on our side. Let us know if anything else comes up.",
        "Everything should be working again. I am marking this ticket as resolved.",
        "Your request has been completed. Thanks for your patience.",
        "We replaced the faulty component and tested it together with you. Resolving this ticket."
    ];

    public static readonly string[] CustomerThanks =
    [
        "Works perfectly now, thank you!",
        "Confirmed, the problem is gone. Thanks for the help.",
        "Great, thanks a lot for the quick fix.",
        "All good on my side. You can close the ticket."
    ];

    public static readonly string[] InternalNotes =
    [
        "Checked the logs: same error as last month's incident. Linking to the known issue.",
        "Customer is a key account - please prioritise.",
        "Waiting for the vendor to confirm the patch; follow up on Friday.",
        "Reproduced on a test machine; the workaround is to clear the local cache.",
        "Second-line: please check the firewall rule that was changed on Tuesday."
    ];

    public static readonly (string FileName, string ContentType)[] AttachmentFiles =
    [
        ("error-log.txt", "text/plain"),
        ("screenshot.png", "image/png"),
        ("system-info.json", "application/json"),
        ("event-viewer-export.csv", "text/csv")
    ];

    // 16x16 solid-blue PNG
    public const string ScreenshotPngBase64 =
        "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAIAAACQkWg2AAAAF0lEQVR4nGPgzftLEmIY1TCq4e+w1QAA9LJ4EFW3F0UAAAAASUVORK5CYII=";
}
