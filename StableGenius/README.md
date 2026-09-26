# The Very Stable Genius Test™

A satirical quiz where nobody can lose. Every answer gets praise, the score only goes up, and the official
answer appears only after two deliberate confirmations. It is decorated like a gilded palace on purpose:
white marble, and gold on everything. That means rococo corners, a shell crest on every frame, fluted
pilasters, a crystal chandelier, laurel wreaths, a Greek-key trim, gold-leaf confetti and gold-tinted emoji
(eagles, cherubs, even the brain).

It's a parody of superlative-heavy bragging. No real person's name, photo or logo is used.

> **Developed further with vibe coding.** The other samples in this repository were not touched after their
> generating session completed. This one was developed a little further afterwards through vibe coding:
> IIS hosting under a sub-path, a bigger question bank and a handful of gameplay tweaks. So it shows more
> than a single session produced.

## Run it

| Part | Port | Launch entry (`C:\Projects\AI\.claude\launch.json`) | Manual |
|---|---|---|---|
| API (.NET 10, Regira Entities, SQLite) | 5711 | `genius-api` | `dotnet run --project GeniusTest.Api --launch-profile http` |
| SPA (Vue 3, @regira/modules) | 5712 | `genius-web` | `npm --prefix GeniusTest.Web run dev` |

- Play: http://localhost:5712/
- Staff area: http://localhost:5712/admin
- API docs: http://localhost:5711/scalar

The SPA calls `/api/*`, and the Vite dev server proxies that to the API, stripping `/api` (the API's own
routes have no prefix; in IIS the application path plays that role). The database (`GeniusTest.Api/genius.db`)
is disposable. Reset it with `dotnet run -- --ResetDatabase=true` to reseed from the CSV files below.

## Deploy (IIS)

In production the quiz runs at `https://samples.regira.com/stablegenius/play/`, with the API and the SPA on
one IIS site, side by side in a `StableGenius` folder:

| Folder | IIS node | Contents |
|---|---|---|
| `StableGenius\client` | virtual directory `/stablegenius/play` | `npm --prefix GeniusTest.Web run build` → `dist\` |
| `StableGenius\api` | application `/stablegenius/api` (app pool: No Managed Code) | `dotnet publish GeniusTest.Api -c Release` |

- **Paths:** the build path (`/stablegenius/play/`) is set in `vite.config.ts`, the API address in
  `public/config.json` (`production`).
- **Refresh:** `public/web.config` (copied into `dist\`) needs the IIS URL Rewrite module. It serves
  `index.html` for deep links, so a refresh works, and sets the MIME types for `.json`, `.csv` and the fonts.
- **Database:** the app pool account needs write access to `StableGenius\api`, where the API creates `genius.db`.
- **Content:** the published API ships `Data\Seeding\*.csv`, which it reads at startup. An existing database
  keeps its questions; press **RESET** in `/admin` to load new CSV content.

## Initial data (CSV)

All initial content lives in `GeniusTest.Api/Data/Seeding/*.csv`, not in code:

- **Format:** semicolon-separated, UTF-8 with BOM (opens cleanly in Excel), first row is the header.
- **Reader:** `Regira.Office.Csv.CsvHelper`.
- **Validation:** the seeder goes through the entity services, so a negative or spoiling text in a CSV
  stops startup with the same 400-style message the admin UI would show.

| File | Columns | Used for |
|---|---|---|
| `questions.csv` | `Key;Title;Emoji;Type;Category;CorrectNumber;Tolerance;MinValue;MaxValue;Unit;RevealNote;AlwaysAsked` | question bank (seeded into an empty DB); `AlwaysAsked=true` puts a question in every game, right after the opening self-rating (the gender question) |
| `question-options.csv` | `QuestionKey;Text;IsCorrect;CustomReaction;IsUnreachable;PraiseAs` | answer options, linked to `questions.csv` by `Key`, in file order; `IsUnreachable=true` shows a small button centred below the answers that hops away from every attempt and that the API refuses; `PraiseAs` is how the praise refers to the answer: a correct one gets a "Confirmed" praise quoting it ("Male" → "Alpha Male. Confirmed.", "Female" → "Soccer mom, a.k.a. Karen"); a `CustomReaction` on a correct option always wins over that (gender: "Other" → "You don't think in genders. You think in success!") |
| `reactions.csv` | `Part;Strategy;Text` | the 150 praise templates (seeded into an empty DB) |
| `titles.csv` | `Level;Title;IsFinal` | the praise-inflation title ladder, read at startup |
| `legends.csv` | `Name` | the greats who fill the leaderboard, read at startup |
| `game-texts.csv` | `Key;Text` | every text the API puts on a game screen: bonus labels, "Honourable mention", default name and honorific, friendly validation messages (keys in `GameTextKeys.cs`; a test checks each one has a row) |

`Type`, `Category`, `Part` and `Strategy` take the enum names (`Choice`, `Geography`, `Spin`, `Landslide`, …).
Seeding only fills empty tables, so after the first run the questions and reactions are edited in `/admin`
(or reset from the CSVs, see below). Titles, legends and game texts are read at startup and on every reset.

All texts on the game screens, both the jokes and the fixed headings and labels, are in
**`GeniusTest.Web/public/data/screen-texts.csv`**, which has the
columns `Screen;Group;Text;Detail`. The SPA serves this file itself (next to `translations.json`) and
reads it once at startup, so the error pages keep their jokes even when the API is down.

- **How groups work:** each `Group` is a pool, and the screen picks one line at random. Adding a row adds
  variety; no code change is needed.
- **Groups:**
  - `headline` (news ticker)
  - `honorific` (in file order)
  - `startLabel`
  - `testimonial` (`Detail` = author)
  - `submitLabel`, `skipLabel`, `nextLabel`, `resultsLabel`
  - `revealAsk1`, `revealAsk2`, `revealKeepLabel`, `revealBackLabel`
  - `startFailed`, `answerFailed`, `finishFailed`, `revealFailed` (the apologetic toasts)
  - `errorEmoji`, `errorTitle`, `errorMessage` (`Detail` = 401/403/404/500)
  - `errorExcuse`
- **Fixed labels:** each fixed heading or label is a group with a single row (`landingQuestion`, `certTitle`, `scoreLabel`, …).
  Texts may contain `{placeholders}` (`certIq` → `…with an IQ of {iq}`). On the certificate the filled-in values are bold.
- **If a label is missing:** that spot shows the group name, and the console warns `screen-texts.csv has no row for group "…"`.

Tests: `dotnet test GeniusTest.slnx` (21 tests on the spin engine, the positivity rules, the reset gate and the CSV seed files).

## How it works

**Flow:** landing (name + honorific) → "measuring your genius… 159% ⚠ SCALE EXCEEDED" → 10 questions →
results with IQ, the Dunning-Kruger chart, a printable certificate and a leaderboard.

- **Questions**
  - The first question is always a self-rating ("how smart are you?"). Then two number questions and seven
    multiple-choice questions.
  - Everything is one tap: huge buttons, +/- buttons instead of typing, and a "skip (power move)" option.
- **Spin engine** (`GeniusTest.Api/Services/Spin/SpinDoctor.cs`)
  - Picks a *strategy* that fits the answer, then an opener, a spin and a closer this game hasn't used yet.
  - Strategies:
    - `Correct`
    - `BetterThanReality` ("it isn't the capital… yet")
    - `TopPercent` ("only 4% dared")
    - `Landslide` ("62% agree with you")
    - `ThinkBig` / `LeanAndEfficient` (number too high / too low)
    - `Rigged`, `JealousExperts`, `AheadOfScience`
    - `TooModest` (ratings)
    - `PowerMove` (skips)
    - hand-written `Custom` reactions per option (always used on a correct option, most of the time on another)
    - `Legendary` (3%: "the Nobel committee is on line 1")
  - The engine never receives the official answer, so it cannot spoil it.
- **Scoring:** points never go down. An original answer earns more than a correct one. A multiplier grows
  every question ("praise inflation"), and so does the title (`titles.csv`): Smart Person → … → Supreme
  Intellect of the Known Multiverse → *Best Person Ever, Period*.
- **Statistics:** "x% of players…" uses real answer statistics once 12 or more people have answered a
  question. Before that it makes up plausible numbers. Either way the number is spun as a compliment.
- **Revealing the answer:** a tiny grey "show the 'official' answer (boring)" link opens the only tasteful
  element on the site, the library's plain `DefaultModal`. The player has to:
  1. confirm once;
  2. confirm again (that button runs away from the cursor the first time).

  The answer then appears in small print "according to so-called experts", followed by +50 courage points.
- **Positivity guard** (`PositivityGuard.cs`): the API refuses (400) any praise template or custom reaction
  that contains *wrong / incorrect / sorry / fail / …*, that uses an unknown placeholder such as
  `{correct}`, or that mentions the official answer.
- **Error pages** (`/401`, `/403`, `/404`, `/500`, plus a catch-all and a Vue `errorHandler`): they never
  blame the player. Examples: "This page wasn't smart enough to exist", "Our servers couldn't handle your
  brilliance", "Our lawyers confirm you did everything perfectly." In-game error messages are pink and
  gold rather than red.
- **Leaderboard:** you are #1 on your own leaderboard, even when someone else scored more ("Rankings
  certified by you"). History's greats fill the gap until real rivals exist.

## Resetting the content

The staff dashboard (`/admin`) has a big red **RESET** button (`POST /api/content/reset`). It only works after
answering **3 random quiz questions correctly** (the official answers, for once), shown one at a time:

- **The questions:** they come from `GET /api/content/reset-challenge`, and only questions that have a wrong
  answer are used, so no self-ratings, no gender question and no "every option is right" questions.
- **Checking:** the server checks all three answers together. Any wrong answer resets nothing, doesn't say
  which one was wrong, and brings three new questions.
- **A small bank:** with fewer qualifying questions it asks those; with none, the reset is allowed, so a
  broken bank can still be reset.

Once past the questions, the reset does the following:

- **Reads first:** it reads and validates every CSV before touching anything. A broken file gives a message
  and changes nothing.
- **Reseeds:** in one transaction it empties the questions, options and praise templates and reseeds them
  from the CSVs. It also reloads the titles, legends and game texts, so no restart is needed.
- **Leaves games alone:** games are never touched. Every game saves its own copy of its 10 questions when it
  starts (`Game.QuestionsJson`), so players who are mid-game carry on undisturbed. Finished games keep their
  place on the leaderboard.

This also means editing or deleting a question in `/admin` never changes a game someone is already playing.

## Staff area (`/admin`)

The question form has an **emoji picker**: the button next to the emoji field opens a searchable, grouped grid.
The praise-template form has the same picker next to its text; there it inserts the emoji at the cursor.
Its list is **`GeniusTest.Web/public/data/emoji-picker.csv`** (`Group;Emoji;Keywords`); add a row to offer
another emoji. Typing or pasting any emoji into the field still works.

These are the scaffolded Regira slices, re-themed white and gold:

| Entity | Registration | Notes |
|---|---|---|
| `Question` | simple (`For<Question, int, QuestionSearchObject>`) | owned `Options` via `e.Related()`, edited as a table in the form |
| `Reaction` | simple | modal form; the praise-template bank |
| `Game` | complex (typed sort by score + `Answers` include) | only the player name is editable; scores are `[ServerOwned]` and written by `PlayService` alone |

Budget: 2 of 5 simple and 1 of 2 complex slots (free tier).

The staff area has no authentication; that is a deliberate choice. Anyone who knows the URL can edit
questions or press the reset button. If that ever changes, add the Regira auth plugin
(`SelfHostingApiWithAuth` + `vue/auth`).

## Layout

```
GeniusTest.Api/        Program.cs → Infrastructure/HostingExtensions.cs
  Entities/            Questions (+ QuestionPrepper), Reactions, Games
  Services/Spin/       SpinDoctor, PositivityGuard, GameContent (titles + legends from CSV)
  Services/Play/       PlayService + contracts (/api/play: start, answer, reveal, finish)
  Data/Seeding/        *.csv (all initial data), SeedFiles (CSV reader), GeniusSeeder
GeniusTest.Web/
  src/game/            api, store, effects (gold-leaf rain + WebAudio sounds), kitsch.scss (palace theme),
                       ornaments/ (SVG corners, shell crest, divider, Greek key, trellis, marble), components, views
  src/entities/        scaffolded admin slices (questions, reactions, games)
GeniusTest.Tests/      spin engine, positivity and CSV seed-file tests
```
