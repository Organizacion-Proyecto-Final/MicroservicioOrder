using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class FacturePagedResponseDto<T>
    {
        public List<T> Items { get; set; } = new();

        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        public int TotalItems { get; set; }
        public int TotalPages { get; set; }

        // opcional pero MUY útil para tu UI
        public int From => (PageNumber - 1) * PageSize + 1;
        public int To => Math.Min(PageNumber * PageSize, TotalItems);

        // resumen para botones (Pagadas / Pendientes)
       // public Dictionary<string, int> StatusCounts { get; set; } = new();
    }
}
