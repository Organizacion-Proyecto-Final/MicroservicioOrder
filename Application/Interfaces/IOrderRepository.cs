using OrderService.Application.DTOs;
using OrderService.Domain.Entities;

namespace OrderService.Application.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Order?> GetByIdForReadAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyCollection<OrderSummaryDto> Orders, int TotalCount)> GetPagedSummariesAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<(IReadOnlyCollection<OrderSummaryDto> Orders, int TotalCount)> GetPagedSummariesByStatusAsync(int statusId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<(IReadOnlyCollection<OrderSummaryDto> Orders, int TotalCount)> GetPagedSummariesByTableAsync(Guid tableId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<(IReadOnlyCollection<OrderSummaryDto> Orders, decimal Total, bool CanClose)> GetActiveSummaryByTableAsync(Guid tableId, CancellationToken cancellationToken = default);
    Task<bool> HasActiveOrdersForTableAsync(Guid tableId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Order>> GetActiveWithReadyItemsAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Order order, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<Guid, string>> GetActiveStatusNamesByTableIdsAsync(IEnumerable<Guid> tableIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<Guid, Guid>> GetActiveWaiterIdsByTableIdsAsync(IEnumerable<Guid> tableIds, CancellationToken cancellationToken = default);
    Task<bool> HasAnyOrderForTableAsync(Guid tableId, CancellationToken cancellationToken = default);
    Task<List<Order>> GetByIdsAsyncForFacturation(IEnumerable<Guid> ids, CancellationToken ct = default);
}
