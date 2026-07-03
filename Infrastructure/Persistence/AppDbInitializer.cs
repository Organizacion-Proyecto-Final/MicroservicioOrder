using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Persistence;

public static class AppDbInitializer
{
    // Se incrementó el volumen para completar bases que ya tenían la versión
    // anterior de la seed y reforzar una distribución semanal más realista.
    private const int TargetInvoiceCount = 600;
    private const int TargetTodayCount = 8;

    private static readonly (string Name, decimal Price, int Weight)[] MetricProducts =
    [
        ("Hamburguesa completa", 10_000m, 18),
        ("Pizza Muzzarella", 12_000m, 16),
        ("Papas fritas", 6_000m, 14),
        ("Milanesa Napolitana", 14_000m, 10),
        ("Coca Cola", 3_000m, 13),
        ("Empanadas", 1_500m, 8),
        ("Lomito completo", 15_000m, 9),
        ("Agua mineral", 2_000m, 6),
        ("Cerveza", 3_500m, 11),
        ("Helado", 4_000m, 5),
        ("Ravioles", 11_000m, 7),
        ("Asado", 20_000m, 6),
        ("Tostado", 7_000m, 4)
    ];

    public static async Task InitializeAsync(
        AppDbContext context,
        CancellationToken cancellationToken = default)
    {
        await SeedTablesAsync(context, cancellationToken);
        await SeedFacturasAsync(context, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedTablesAsync(
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        var existingTables = await context.Tables
            .ToDictionaryAsync(table => table.Number, cancellationToken);

        foreach (var seed in BuildTables())
        {
            if (existingTables.ContainsKey(seed.Number))
                continue;

            await context.Tables.AddAsync(
                Table.Create(seed.Number, seed.SeatCount, seed.Location, seed.IsEnabled),
                cancellationToken);
        }
    }

    private static List<TableSeed> BuildTables() =>
    [
        new("1", 2, "Salón", true),
        new("2", 2, "Salón", true),
        new("3", 4, "Salón", true),
        new("4", 4, "Salón", true),
        new("5", 4, "Salón", true),
        new("6", 6, "Salón", true),
        new("7", 2, "Patio", true),
        new("8", 4, "Patio", true),
        new("9", 4, "Patio", true),
        new("10", 6, "Patio", true),
        new("11", 2, "Barra", true),
        new("12", 2, "Barra", true),
        new("13", 8, "Salón", true),
        new("14", 4, "Terraza", true)
    ];

    private static async Task SeedFacturasAsync(
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        // La aplicación trabaja en UTC y la interfaz muestra las fechas en UTC-3.
        var utcNow = DateTime.UtcNow;
        var argentinaNow = utcNow.AddHours(-3);
        var argentinaToday = argentinaNow.Date;
        var todayUtc = argentinaToday.AddHours(3);

        var existingCount = await context.Facturas.CountAsync(cancellationToken);
        var existingTodayCount = await context.Facturas.CountAsync(
            factura => factura.Date >= todayUtc && factura.Date <= utcNow,
            cancellationToken);

        var desiredTodayCount = argentinaNow.Hour < 13 ? 0 : TargetTodayCount;
        var todayMissing = Math.Max(0, desiredTodayCount - existingTodayCount);
        var historyMissing = Math.Max(0, TargetInvoiceCount - existingCount - todayMissing);

        if (todayMissing == 0 && historyMissing == 0)
            return;

        // Una semilla estable por día produce datos repetibles durante el desarrollo.
        var seed = argentinaToday.Year * 10_000
                   + argentinaToday.Month * 100
                   + argentinaToday.Day;
        var random = new Random(seed);
        var facturas = new List<Factura>(todayMissing + historyMissing);

        for (var i = 0; i < todayMissing; i++)
        {
            var localDate = GetPastBusinessTimeToday(random, argentinaNow);

            facturas.Add(BuildFactura(random, localDate.AddHours(3)));
        }

        for (var i = 0; i < historyMissing; i++)
        {
            // Se concentra el 65 % en el mes actual para alimentar las métricas mensuales.
            var daysAgo = random.NextDouble() < 0.65
                ? GetWeightedDaysAgo(random, argentinaToday, 1, 31)
                : GetWeightedDaysAgo(random, argentinaToday, 31, 91);

            var localDate = argentinaToday
                .AddDays(-daysAgo)
                .AddHours(GetBusinessHour(random))
                .AddMinutes(random.Next(0, 60));

            facturas.Add(BuildFactura(random, localDate.AddHours(3)));
        }

        await context.Facturas.AddRangeAsync(facturas, cancellationToken);
    }

    private static Factura BuildFactura(Random random, DateTime utcDate)
    {
        var factura = new Factura
        {
            TableName = random.Next(1, 15).ToString(),
            Date = DateTime.SpecifyKind(utcDate, DateTimeKind.Utc),
            IsPaid = random.NextDouble() < 0.82,
            Details = new List<FacturaDetail>()
        };

        var detailCount = random.Next(1, 5);
        var usedProducts = new HashSet<string>();

        while (factura.Details.Count < detailCount)
        {
            var product = PickWeightedProduct(random);

            if (!usedProducts.Add(product.Name))
                continue;

            factura.Details.Add(new FacturaDetail
            {
                ProductName = product.Name,
                Quantity = random.Next(1, 4),
                Price = product.Price
            });
        }

        factura.Total = factura.Details.Sum(detail => detail.Price * detail.Quantity);
        return factura;
    }

    private static (string Name, decimal Price, int Weight) PickWeightedProduct(Random random)
    {
        var value = random.Next(MetricProducts.Sum(product => product.Weight));

        foreach (var product in MetricProducts)
        {
            if (value < product.Weight)
                return product;

            value -= product.Weight;
        }

        return MetricProducts[^1];
    }

    private static int GetBusinessHour(Random random)
    {
        // En gastronomía argentina se concentra la actividad en el almuerzo
        // (13 a 15) y, especialmente, durante la cena (20 a 24).
        int[] hours = [11, 12, 13, 14, 15, 16, 18, 19, 20, 21, 22, 23];
        int[] weights = [1, 3, 12, 16, 11, 3, 1, 4, 13, 18, 20, 16];
        var value = random.Next(weights.Sum());

        for (var i = 0; i < hours.Length; i++)
        {
            if (value < weights[i])
                return hours[i];

            value -= weights[i];
        }

        return 21;
    }

    private static DateTime GetPastBusinessTimeToday(Random random, DateTime argentinaNow)
    {
        int[] businessHours = [13, 14, 15, 20, 21, 22, 23];
        int[] weights = [12, 16, 11, 13, 18, 20, 16];
        var available = businessHours
            .Select((hour, index) => new { Hour = hour, Weight = weights[index] })
            .Where(slot => slot.Hour <= argentinaNow.Hour)
            .ToList();

        var value = random.Next(available.Sum(slot => slot.Weight));
        var selectedHour = available[^1].Hour;

        foreach (var slot in available)
        {
            if (value < slot.Weight)
            {
                selectedHour = slot.Hour;
                break;
            }

            value -= slot.Weight;
        }

        var maximumMinute = selectedHour == argentinaNow.Hour
            ? argentinaNow.Minute
            : 59;

        return argentinaNow.Date
            .AddHours(selectedHour)
            .AddMinutes(random.Next(0, maximumMinute + 1));
    }

    private static int GetWeightedDaysAgo(
        Random random,
        DateTime argentinaToday,
        int minimumDaysAgo,
        int maximumDaysAgoExclusive)
    {
        // Domingo = 0. Viernes, sábado y domingo reciben claramente más peso.
        int[] weekdayWeights = [18, 4, 5, 6, 9, 17, 22];
        var candidates = Enumerable
            .Range(minimumDaysAgo, maximumDaysAgoExclusive - minimumDaysAgo)
            .Select(daysAgo => new
            {
                DaysAgo = daysAgo,
                Weight = weekdayWeights[(int)argentinaToday.AddDays(-daysAgo).DayOfWeek]
            })
            .ToList();

        var value = random.Next(candidates.Sum(candidate => candidate.Weight));

        foreach (var candidate in candidates)
        {
            if (value < candidate.Weight)
                return candidate.DaysAgo;

            value -= candidate.Weight;
        }

        return candidates[^1].DaysAgo;
    }

    private sealed record TableSeed(
        string Number,
        int SeatCount,
        string Location,
        bool IsEnabled);
}
