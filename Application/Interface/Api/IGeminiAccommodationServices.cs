using Application.DTOs.Accommodation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.Api
{
    public interface IGeminiAccommodationServices
    {
        Task<(IEnumerable<AccommodationPost> Posts, IEnumerable<AccommodationReview> Reviews)> GetContextDataForAIAsync();
        Task<ResponseSearchAIDto> GenerateSearchResponseAsync(
            string userQuery,
            IEnumerable<AccommodationPost> posts,
            IEnumerable<AccommodationReview> reviews);
    }
}
