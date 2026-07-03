using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.UseCases.Facturation;
using Application.DTOs;
using Domain.Enums;

namespace Application.UseCases.Facturation.Queries
{
    public class GetFacturasHandler : IGetFacturasHandler
    {
        private readonly IFacturationRepository _facturasRepository;

        public GetFacturasHandler(IFacturationRepository facturationRepository)
        {
            _facturasRepository = facturationRepository;
        }

        public async Task<FacturePagedResponseDto<FacturaDto>> Handle(GetFacturasQuery query)
        {
            var (items, total) = await _facturasRepository.GetFacturasAsync(
                query.PageNumber,
                query.PageSize,
                query.FromDate,
                query.ToDate,
                query.Filter
            );

            var dtoItems = items.Select(x => new FacturaDto
            {
                Id = x.Id,
                TableName = x.TableName,
                Date = x.Date,
                IsPaid = x.IsPaid,
                Total = x.Total,
                Details = x.Details.Select(d => new FacturaDetailDto
                {
                    Quantity = d.Quantity,
                    ProductName = d.ProductName,
                    Price = d.Price
                }).ToList()
            }).ToList();

            return new FacturePagedResponseDto<FacturaDto>
            {
                Items = dtoItems,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalItems = total,
                TotalPages = (int)Math.Ceiling(total / (double)query.PageSize)
            };
        }
    }
}
