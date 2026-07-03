using Application.DTOs;
using Application.Interfaces;

namespace Application.UseCases.Facturation.Queries;

public sealed class GetFacturationMetricsHandler : IGetFacturationMetricsHandler
{
    private readonly IFacturationRepository _facturationRepository;

    public GetFacturationMetricsHandler(IFacturationRepository facturationRepository)
    {
        _facturationRepository = facturationRepository;
    }

    public async Task<FacturationMetricsDto> Handle(
        CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;
        var argentinaNow = utcNow.AddHours(-3);
        var todayUtc = argentinaNow.Date.AddHours(3);
        var monthStartUtc = new DateTime(
            argentinaNow.Year,
            argentinaNow.Month,
            1).AddHours(3);
        var daysSinceMonday = ((int)argentinaNow.DayOfWeek + 6) % 7;
        var weekStartUtc = argentinaNow.Date
            .AddDays(-daysSinceMonday)
            .AddHours(3);
        var queryStartUtc = monthStartUtc < weekStartUtc
            ? monthStartUtc
            : weekStartUtc;

        var invoices = await _facturationRepository.GetForMetricsAsync(
            queryStartUtc,
            utcNow,
            cancellationToken);

        var monthInvoices = invoices
            .Where(invoice => invoice.Date >= monthStartUtc)
            .ToList();
        var weekInvoices = invoices
            .Where(invoice => invoice.Date >= weekStartUtc)
            .ToList();
        var paidInvoices = monthInvoices.Where(invoice => invoice.IsPaid).ToList();
        var paidToday = paidInvoices
            .Where(invoice => invoice.Date >= todayUtc)
            .ToList();

        var productMetrics = paidInvoices
            .SelectMany(invoice => invoice.Details)
            .GroupBy(detail => detail.ProductName)
            .Select(group => new ProductMetricDto
            {
                ProductName = group.Key,
                Quantity = group.Sum(detail => detail.Quantity),
                Revenue = group.Sum(detail => detail.Quantity * detail.Price)
            })
            .ToList();

        var hourlyCounts = monthInvoices
            .GroupBy(invoice => invoice.Date.AddHours(-3).Hour)
            .ToDictionary(group => group.Key, group => group.Count());

        var weekdayCounts = weekInvoices
            .GroupBy(invoice => (int)invoice.Date.AddHours(-3).DayOfWeek)
            .ToDictionary(group => group.Key, group => group.Count());

        return new FacturationMetricsDto
        {
            TodayTotal = paidToday.Sum(invoice => invoice.Total),
            TodayCount = paidToday.Count,
            MonthTotal = paidInvoices.Sum(invoice => invoice.Total),
            MonthCount = paidInvoices.Count,
            TopProducts = productMetrics
                .OrderByDescending(metric => metric.Quantity)
                .ThenBy(metric => metric.ProductName)
                .Take(10)
                .ToList(),
            TopRevenueProducts = productMetrics
                .OrderByDescending(metric => metric.Revenue)
                .ThenBy(metric => metric.ProductName)
                .Take(10)
                .ToList(),
            HourlyConcurrency = Enumerable.Range(0, 24)
                .Select(hour => new HourlyMetricDto
                {
                    Hour = hour,
                    InvoiceCount = hourlyCounts.GetValueOrDefault(hour)
                })
                .ToList(),
            WeeklyConcurrency = new[] { 1, 2, 3, 4, 5, 6, 0 }
                .Select(day => new WeekdayMetricDto
                {
                    DayOfWeek = day,
                    InvoiceCount = weekdayCounts.GetValueOrDefault(day)
                })
                .ToList()
        };
    }
}
