using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Enums;

namespace Application.Interfaces
{
    public interface IFacturationRepository
    {
        Task<(List<Factura> Items, int Total)> GetFacturasAsync(
            int pageNumber,
            int pageSize,
            DateTime? fromDate,
            DateTime? toDate,
            PaymentFilter filter);

        Task<bool> MarkAsPaidAsync(int facturaId);
        Task<bool> MarkLatestAsPaidByTableAsync(
            string tableName,
            CancellationToken cancellationToken = default);
        Task<Factura> CreateAsync(Factura factura);
        Task AddAsync(Factura factura, CancellationToken ct = default);
        Task<List<Factura>> GetForMetricsAsync(
            DateTime fromUtc,
            DateTime toUtc,
            CancellationToken cancellationToken = default);
    }
}
