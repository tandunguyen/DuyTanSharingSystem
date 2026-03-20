using Application.DTOs.Accommodation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.CQRS.Queries.Accommodation
{
    public class SearchAIQueryHandler : IRequestHandler<SearchAIQuery, ResponseModel<ResponseSearchAIDto>>
    {
        private readonly IGeminiAccommodationServices _geminiAccommodationServices;
        public SearchAIQueryHandler(IGeminiAccommodationServices geminiAccommodationServices)
        {
            _geminiAccommodationServices = geminiAccommodationServices;
        }
        public async Task<ResponseModel<ResponseSearchAIDto>> Handle(SearchAIQuery request, CancellationToken cancellationToken)
        {
            // Lấy dữ liệu ngữ cảnh từ dịch vụ Gemini
            var (posts, reviews) = await _geminiAccommodationServices.GetContextDataForAIAsync();
            // Gọi dịch vụ Gemini để tạo phản hồi dựa trên truy vấn của người dùng và dữ liệu ngữ cảnh
            var aiResponse = await _geminiAccommodationServices.GenerateSearchResponseAsync(
                request.Question,
                posts,
                reviews);
            // Trả về phản hồi dưới dạng ResponseModel
            return new ResponseModel<ResponseSearchAIDto>
            {
                Data = aiResponse,
                Success = true,
                Message = "AI response generated successfully."
            };
        }
    }
}
