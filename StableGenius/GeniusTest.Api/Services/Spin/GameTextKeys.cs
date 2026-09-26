namespace GeniusTest.Api.Services.Spin;

/// <summary>
/// Keys into Data/Seeding/game-texts.csv - every text the API itself puts on a game screen.
/// A test asserts that each constant here has a row in the CSV.
/// </summary>
public static class GameTextKeys
{
    public const string DefaultHonorific = "defaultHonorific";
    public const string AnonymousPlayer = "anonymousPlayer";
    public const string NoAnswer = "noAnswer";
    public const string RatingOfficialAnswer = "ratingOfficialAnswer";
    public const string LegendTitle = "legendTitle";

    // the label under the points
    public const string BonusCorrect = "bonusCorrect";
    public const string BonusOriginal = "bonusOriginal";
    public const string BonusPowerMove = "bonusPowerMove";
    public const string BonusTooModest = "bonusTooModest";
    public const string BonusLegendary = "bonusLegendary";

    // used only when the praise-template table has nothing suitable
    public const string FallbackOpener = "fallbackOpener";
    public const string FallbackSpin = "fallbackSpin";
    public const string FallbackCloser = "fallbackCloser";
    public const string FallbackReveal = "fallbackReveal";
    public const string FallbackFinale = "fallbackFinale";

    // friendly validation messages (400/404)
    public const string ErrorEmptyBank = "errorEmptyBank";
    public const string ErrorNotInGame = "errorNotInGame";
    public const string ErrorQuestionRetired = "errorQuestionRetired";
    public const string ErrorPickOption = "errorPickOption";
    public const string ErrorUnreachable = "errorUnreachable";
    public const string ErrorTypeNumber = "errorTypeNumber";
    public const string ErrorRateYourself = "errorRateYourself";
    public const string ErrorAnswerFirst = "errorAnswerFirst";
    public const string ErrorNotSoFast = "errorNotSoFast";
    public const string ErrorGameNotFound = "errorGameNotFound";
}
