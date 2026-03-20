using Application.DTOs.AccommodationReview;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.CQRS.Queries.AccommodationReviews
{
    public class GetAccommodationReviewsQuery : IRequest<ResponseModel<GetAllAccommodationReviewDto>>
    {
        public Guid AccommodationPostId { get; set; }
        public Guid? lastAccommodationReviewId { get; set; } // Cursor
        public int PageSize { get; set; } = 10;
    }
}
