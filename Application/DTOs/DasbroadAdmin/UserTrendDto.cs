using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.DasbroadAdmin
{
    public class UserTrendDto
    {
        public List<string> Labels { get; set; } = new List<string>(); // Khởi tạo mặc định
        public List<int> Data { get; set; } = new List<int>(); // Khởi tạo mặc định
    }
}
