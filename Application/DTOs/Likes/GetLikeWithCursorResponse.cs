using Application.DTOs.Post;
using Application.DTOs.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Likes
{
    public class GetLikeWithCursorResponse
    {
        public int LikeCount { get; set; } // Tổng số lượt like
        public bool IsLikedByCurrentUser { get; set; } // Người dùng hiện tại đã thích bài viết hay chưa
        public List<UserPostDto> LikedUsers { get; set; } = new();
        public Guid? NextCursor { get; set; }
    }
}
