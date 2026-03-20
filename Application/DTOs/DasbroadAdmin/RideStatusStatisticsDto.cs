using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.DasbroadAdmin
{
    public class RideStatusStatisticsDto
    {
        public string TimeLabel { get; set; }
        public int RejectedCount { get; set; }
        public int AcceptedCount { get; set; }
        public int CompletedCount { get; set; }
    }
}
