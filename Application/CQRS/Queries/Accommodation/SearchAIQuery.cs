using Application.DTOs.Accommodation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.CQRS.Queries.Accommodation
{
    public class SearchAIQuery : IRequest<ResponseModel<ResponseSearchAIDto>>
    {
        public string Question { get; set; } = string.Empty;
    }
}
