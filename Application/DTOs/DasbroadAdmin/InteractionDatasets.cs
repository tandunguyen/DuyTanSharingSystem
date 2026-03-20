using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.DasbroadAdmin
{
    public class InteractionDatasets
    {
        public List<int> Likes { get; set; } = new List<int>(); // Khởi tạo mặc định
        public List<int> Comments { get; set; } = new List<int>(); // Khởi tạo mặc định
        public List<int> Shares { get; set; } = new List<int>(); // Khởi tạo mặc định
    }
}
