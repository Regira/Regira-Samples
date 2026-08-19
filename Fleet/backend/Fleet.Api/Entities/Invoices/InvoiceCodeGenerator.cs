using Fleet.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Fleet.Api.Entities.Invoices;

// Mints sequential "INV-{year}-{00001}" codes. Primed from the highest existing code per year
// (never a row count -- see entities.patterns -> Server-generated sequential codes), then
// incremented in memory. Registered as a singleton: correct for a single-process deployment.
public class InvoiceCodeGenerator(IServiceScopeFactory scopeFactory)
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly Dictionary<int, int> _last = new();

    public async Task<string> Next(int year)
    {
        var prefix = $"INV-{year}-";
        await _lock.WaitAsync();
        try
        {
            if (!_last.TryGetValue(year, out var seq))
            {
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<FleetDbContext>();
                var highest = await db.Invoices
                    .Where(x => x.Code != null && x.Code.StartsWith(prefix))
                    .OrderByDescending(x => x.Code)
                    .Select(x => x.Code!)
                    .FirstOrDefaultAsync();
                seq = highest is null ? 0 : int.Parse(highest[prefix.Length..]);
            }
            _last[year] = ++seq;
            return $"{prefix}{seq:D5}";
        }
        finally
        {
            _lock.Release();
        }
    }
}
