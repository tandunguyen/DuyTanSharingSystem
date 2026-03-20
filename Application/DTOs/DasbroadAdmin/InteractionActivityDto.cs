using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.DasbroadAdmin
{
    public class InteractionActivityDto
    {
        public List<string> Labels { get; set; } = new List<string>(); // Khởi tạo mặc định
        public InteractionDatasets Datasets { get; set; } = new InteractionDatasets(); // Khởi tạo mặc định
    }
}
