using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.DasbroadAdmin
{
    public class RatingStatisticsDto
    {
        public double PoorPercentage { get; set; }
        public double AveragePercentage { get; set; }
        public double GoodPercentage { get; set; }
        public double ExcellentPercentage { get; set; }
    }
}
