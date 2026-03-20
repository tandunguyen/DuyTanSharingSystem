using Application.DTOs.StudyMaterial;
using MediatR;

namespace Application.CQRS.Queries.StudyMaterials
{
    public class GetAllReviewQuery : IRequest<ResponseModel<GetAllStudyMaterialReviewDto>> // Sửa thành GetAllStudyMaterialReviewDto
    {
        // Loại bỏ NextCursor và Reviews không cần thiết trong Query Command
        public Guid? LastStudyMaterialRatingId { get; set; } // Đổi tên cho rõ ràng: Cursor - ID của bài đánh giá cuối cùng của trang trước
        public Guid StudyMaterialId { get; set; } // ID của tài liệu học tập cần lấy đánh giá

        // Thay thế logic PageSize ngầm định bằng thuộc tính PageSize rõ ràng
        public int PageSize { get; set; } = 10; // Đặt giá trị mặc định cho PageSize
    }
}