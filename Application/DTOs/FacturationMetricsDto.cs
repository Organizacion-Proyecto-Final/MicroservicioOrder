namespace Application.DTOs;

public sealed class FacturationMetricsDto
{
    public decimal TodayTotal { get; init; }
    public int TodayCount { get; init; }
    public decimal MonthTotal { get; init; }
    public int MonthCount { get; init; }
    public List<ProductMetricDto> TopProducts { get; init; } = [];
    public List<ProductMetricDto> TopRevenueProducts { get; init; } = [];
    public List<HourlyMetricDto> HourlyConcurrency { get; init; } = [];
    public List<WeekdayMetricDto> WeeklyConcurrency { get; init; } = [];
}

public sealed class ProductMetricDto
{
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal Revenue { get; init; }
}

public sealed class HourlyMetricDto
{
    public int Hour { get; init; }
    public int InvoiceCount { get; init; }
}

public sealed class WeekdayMetricDto
{
    public int DayOfWeek { get; init; }
    public int InvoiceCount { get; init; }
}
