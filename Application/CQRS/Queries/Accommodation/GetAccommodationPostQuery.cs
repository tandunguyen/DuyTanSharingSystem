using Application.DTOs.Accommodation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.CQRS.Queries.Accommodation
{
    public class GetAccommodationPostQuery : IRequest<ResponseModel<GetAllAccommodationPostDto.AccommodationPostDto>>
    {
        public Guid Id { get; set; }
        public GetAccommodationPostQuery() { }
    }
}
