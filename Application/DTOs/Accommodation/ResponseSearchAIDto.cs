using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Application.DTOs.Accommodation
{
    public class ResponseSearchAIDto
    {
        public string Answer { get; set; } = string.Empty;
        [JsonConverter(typeof(FlexibleListConverter))]
        public List<GetAllAccommodationPostDto.LatLogAccommodation>? ResponseDataAI { get; set; } = new();
    }
}
