using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.FriendShips
{
    public class FriendSuggestionDto
    {
        public Guid Id { get; set; }
        public string? FullName { get; set; }
        public string? ProfilePicture { get; set; }
        public decimal TrustScore { get; set; }
        public DateTime? LastActive { get; set; }
        public int CommonInterests { get; set; }
    }
}
