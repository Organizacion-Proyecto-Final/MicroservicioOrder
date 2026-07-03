using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.UseCases.Facturation.Queries;
using Application.DTOs;

namespace Application.Interfaces
{
    public interface IGetFacturasHandler
    {
        Task<FacturePagedResponseDto<FacturaDto>> Handle(GetFacturasQuery query);
    }
}
