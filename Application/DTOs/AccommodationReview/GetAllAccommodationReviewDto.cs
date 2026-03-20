using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.AccommodationReview
{
    public class GetAllAccommodationReviewDto
    {
        public Guid? NextCursor { get; set; } // Dùng cho phân trang dạng Cursor
        public int TotalCount { get; set; } // Tổng số lượng bài đăng (không phân trang)
        public List<ResponseAccommodationReviewDto.AccommodationReviewDto> Reviews { get; set; } = new();
    }
}
