using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.CQRS.Commands.AccommodationReviews
{
    public  class DeleteAccommodationReviewCommand : IRequest<ResponseModel<bool>>
    {
        [Required]
        public required Guid Id { get; set; }
    }
}
