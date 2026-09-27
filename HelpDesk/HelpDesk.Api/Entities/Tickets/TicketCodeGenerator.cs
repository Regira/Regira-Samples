using HelpDesk.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Api.Entities.Tickets;

/// <summary>
/// Sequential ticket references (HD-000001). Primed once from the highest stored code, then incremented in memory,
/// so a bulk wave never hands out the same number twice. Singleton: one counter per process (single-instance app);
/// the unique index on Code remains the guarantee.
/// </summary>
public class TicketCodeGenerator(IServiceScopeFactory scopeFactory)
{
    public const string Prefix = "HD-";
    private readonly SemaphoreSlim _lock = new(1, 1);
    private int? _last;

    public static string Format(int sequence) => $"{Prefix}{sequence:D6}";

    public async Task<string> Next(CancellationToken token = default)
    {
        await _lock.WaitAsync(token);
        try
        {
            if (_last == null)
            {
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<HelpDeskDbContext>();
                // zero-padded to a fixed width, so the lexical max is the numeric max
                var highest = await db.Tickets.IgnoreQueryFilters().AsNoTracking()
                    .Where(x => x.Code!.StartsWith(Prefix))
                    .OrderByDescending(x => x.Code)
                    .Select(x => x.Code!)
                    .FirstOrDefaultAsync(token);
                _last = highest is null ? 0 : int.Parse(highest[Prefix.Length..]);
            }
            _last++;
            return Format(_last.Value);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>Forget the primed value (after a seeder wrote codes itself).</summary>
    public void Reset() => _last = null;
}
