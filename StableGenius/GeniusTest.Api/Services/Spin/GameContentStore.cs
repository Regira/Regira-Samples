namespace GeniusTest.Api.Services.Spin;

/// <summary>
/// Holds the current <see cref="GameContent"/> (titles, legends, game texts). The reset swaps it atomically;
/// each request resolves the content once (scoped), so a request that is halfway never sees a mix.
/// </summary>
public class GameContentStore(GameContent initial)
{
    private GameContent _current = initial;

    public GameContent Current => Volatile.Read(ref _current);

    public void Replace(GameContent content) => Volatile.Write(ref _current, content);
}
