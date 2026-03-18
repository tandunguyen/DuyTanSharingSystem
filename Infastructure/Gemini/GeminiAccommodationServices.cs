// File: Infrastructure/Gemini/GeminiAccommodationServices.cs
// (Phiên bản cập nhật đầy đủ)

using DuyTanSharingSystem.Infastructure.Gemini;
using Domain.Entities; // 💡 Thêm (để dùng AccommodationPost/Review)
using Domain.Interface; // 💡 Thêm (để dùng IAccommodationPostRepository)
using Microsoft.Extensions.Configuration; // 💡 Thêm
using Microsoft.Extensions.Options; // 💡 Thêm
using System; // 💡 Thêm
using System.Collections.Generic;
using System.IO;
using System.Linq; // 💡 Thêm
using System.Net.Http; // 💡 Thêm
using System.Text; // 💡 Thêm
using System.Text.Json;
using System.Threading.Tasks;
using Application.DTOs.Accommodation;
using Microsoft.AspNetCore.Hosting;
using Domain.Common;

namespace Infrastructure.Gemini
{
    // 💡 Giả định bạn có Interface, nếu không có thì không sao
    public class GeminiAccommodationServices  : IGeminiAccommodationServices
    {
        // Đường dẫn Schema
        private readonly string _schemaFilePath;


        // 💡 Dependencies để gọi API (giống GeminiService2)
        private readonly HttpClient _httpClient;
        private readonly GeminiModel _geminiModel;

        // 💡 Dependencies để lấy dữ liệu (Repositories)
        private readonly IAccommodationPostRepository _postRepo;
        private readonly IAccommodationReviewRepository _reviewRepo;

        // 💡 Cập nhật Constructor
        public GeminiAccommodationServices(
            IHttpClientFactory httpClientFactory,
            IOptions<GeminiModel> geminiModel,
            IAccommodationPostRepository postRepo,
            IAccommodationReviewRepository reviewRepo,
            IWebHostEnvironment env)
        {
            _httpClient = httpClientFactory.CreateClient();
            _geminiModel = geminiModel.Value;
            _postRepo = postRepo;
            _reviewRepo = reviewRepo;
            _schemaFilePath = Path.Combine(
    Directory.GetParent(env.ContentRootPath)?.FullName ?? env.ContentRootPath,
    "Infastructure", "Gemini", "Schema.json"
);

        }
        // Helper method – đặt trong class
        private string GetBasePath(IWebHostEnvironment env)
        {
            // 1. Unit Test: env.ContentRootPath = thư mục test project
            // 2. Dev: env.ContentRootPath = thư mục gốc dự án
            // 3. Publish: env.ContentRootPath = thư mục publish
            return env.ContentRootPath;
        }
        // 1. Hàm Load Schema (Giữ nguyên từ file của bạn)
        private async Task<List<TableModel>> LoadSchemaAsync()
        {
            //E:\DOANTOTNGHIEP\Code\DuyTanSharingSystem\DuyTanSharingSystem\Infastructure\Gemini\Schema.json
            //E:\DOANTOTNGHIEP\Code\DuyTanSharingSystem\Infastructure\Gemini\Schema.json
            //E:\DOANTOTNGHIEP\Code\DuyTanSharingSystem\DuyTanSharingSystem\bin\Debug\net8.0\Infastructure\Gemini\Schema.json
            if (!File.Exists(_schemaFilePath)) //
            {
                throw new FileNotFoundException($"Không tìm thấy file Schema.json tại: {_schemaFilePath}"); //
            }
            try
            {
                string jsonString = await File.ReadAllTextAsync(_schemaFilePath); //
                var schemaList = JsonSerializer.Deserialize<List<TableModel>>(jsonString); //
                return schemaList ?? new List<TableModel>();
            }
            catch (JsonException ex) //
            {
                throw new InvalidDataException("File Schema.json không đúng định dạng JSON.", ex); //
            }
            catch (Exception ex) //
            {
                throw new Exception("Lỗi khi đọc file Schema.json.", ex); //
            }
        }

        // ==================================================================
        // HÀM 1: LẤY DỮ LIỆU (THEO YÊU CẦU CỦA BẠN)
        // =â
        /// <summary>
        /// Lấy dữ liệu ngữ cảnh (Posts và Reviews) để cung cấp cho Gemini.
        /// </summary>
        public async Task<(IEnumerable<AccommodationPost> Posts, IEnumerable<AccommodationReview> Reviews)> GetContextDataForAIAsync()
        {
            // CẢNH BÁO: Không nên lấy TẤT CẢ dữ liệu.
            // Chúng ta sẽ lấy 20 bài đăng mới nhất và 5 review cho mỗi bài
            // để làm ngữ cảnh (context) cho AI.

            // 1. Lấy 20 bài đăng mới nhất (đã bao gồm User)
            // Sử dụng hàm đã có trong AccommodationPostRepository.cs
            var posts = await _postRepo.GetAllAccommodationPostAsync(null, 20); //

            var postIds = posts.Select(p => p.Id).ToList();
            var reviews = new List<AccommodationReview>();

            // 2. Lấy 5 review đầu tiên cho mỗi bài đăng (đã bao gồm User)
            foreach (var id in postIds)
            {
                // Sử dụng hàm đã có trong AccommodationReviewRepository.cs
                var postReviews = await _reviewRepo.GetReviewsByAccommodationPostIdAsync(id, null, 5); //
                reviews.AddRange(postReviews);
            }

            return (posts, reviews);
        }

        // ==================================================================
        // HÀM 2: PHÂN TÍCH AI (THEO YÊU CẦU CỦA BẠN)
        // ==================================================================
        /// <summary>
        /// Nhận câu hỏi, dữ liệu (từ Hàm 1) và Schema (từ LoadSchemaAsync),
        /// sau đó gọi Gemini để phân tích và trả về JSON theo DTO.
        /// </summary>
        public async Task<ResponseSearchAIDto> GenerateSearchResponseAsync(
            string userQuery,
            IEnumerable<AccommodationPost> posts,
            IEnumerable<AccommodationReview> reviews)
        {
            // 1. Load Schema (để AI hiểu cấu trúc)
            var schema = await LoadSchemaAsync();
            string schemaJson = JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });

            // 2. Chuẩn bị dữ liệu (Data Context)
            // Chuyển đổi dữ liệu sang định dạng JSON gọn gàng cho AI
            var dataContext = new
            {
                AvailablePosts = posts.Select(p => new
                {
                    p.Id,
                    p.UserId,
                    p.Title,
                    p.Address,
                    p.Latitude,
                    p.Longitude,
                    p.Price,
                    p.Area,
                    p.RoomType,
                    p.MaxPeople,
                    p.CurrentPeople,
                    PostedBy = p.User?.FullName, // Lấy từ User được Include
                    TrustScore = p.User?.TrustScore,
                    Gender=p.User?.Gender
                }),
                ReviewsForPosts = reviews.Select(r => new
                {
                    r.AccommodationPostId,
                    r.Rating,
                    r.SafetyScore,
                    r.PriceScore,
                    r.Comment,
                    Reviewer = r.User?.FullName // Lấy từ User được Include
                })
            };
            string dataContextJson = JsonSerializer.Serialize(dataContext, new JsonSerializerOptions { WriteIndented = true });

            // 3. Chuẩn bị định dạng JSON Output (cho AI biết cách trả lời)
            string outputFormatJson = @"{
""Answer"": ""(string, câu trả lời bằng ngôn ngữ tự nhiên, giải thích lý do chọn phòng này)"",
""ResponseDataAI"": {
    ""Id"": ""(Guid, Id của phòng được chọn)"",
    ""UserId"": ""(Guid, UserId của chủ phòng)"",
    ""Latitude"": (double),
    ""Longitude"": (double),
    ""Address"": ""(string, địa chỉ phòng)"",
    ""Price"": (decimal),
    ""Status"": ""(string, trạng thái phòng, vd: 'Available')""
},{
""Tiếp tục nếu có nhiều hơn dữ liệu đạt yêu cầu""
}
}";

            // Xử lý nếu không có dữ liệu đầu vào
            if (!posts.Any())
            {
                return new ResponseSearchAIDto
                {
                    Answer = "Rất tiếc, hiện tại không có dữ liệu phòng trọ nào để tôi phân tích. Vui lòng thử lại sau.",
                    ResponseDataAI = null
                };
            }

            // 4. Tạo Prompt
            var prompt = $@"
Bạn là một trợ lý AI thông minh chuyên tư vấn và tìm kiếm phòng trọ tại Đại học Duy Tân.
Mục tiêu của bạn là phân tích câu hỏi của người dùng, đối chiếu với dữ liệu phòng trọ và đánh giá (reviews) được cung cấp để tìm ra **MỘT (1)** phòng trọ phù hợp nhất.
Nếu câu hỏi không liên quan, bạn hãy trả lời 'Tôi xin lỗi, tôi không thể giúp với yêu cầu này.' và không chọn phòng nào.
**BỐI CẢNH (Không cần trả lời):**
1. **Schema Database (JSON):** Đây là cấu trúc 3 bảng (Users, AccommodationPosts, AccommodationReviews) để bạn hiểu ý nghĩa dữ liệu.
```json
{schemaJson}Câu hỏi của người dùng (Query): ""{userQuery}""

Dữ liệu phòng trọ và đánh giá (JSON): Đây là danh sách các phòng trọ và đánh giá liên quan đã được lọc. Bạn phải làm việc CHỈ với dữ liệu này.

JSON

{dataContextJson}
YÊU CẦU:

Phân tích kỹ câu hỏi của người dùng (ví dụ: họ quan tâm đến giá, sự an toàn, khoảng cách, hay tiện nghi).

Duyệt qua 'AvailablePosts' và 'ReviewsForPosts' trong 'Dữ liệu'.

Chọn ra MỘT hoặc nhiều AccommodationPost phù hợp nhất dựa trên câu hỏi.

Nếu người dùng hỏi 'an toàn', hãy ưu tiên phòng có SafetyScore cao trong 'ReviewsForPosts'.

Nếu người dùng hỏi 'giá rẻ', ưu tiên phòng có Price thấp hoặc PriceScore cao.

Nếu không có review, hãy dựa vào thông tin của bài đăng (Title, Content, Price).

Tạo một câu trả lời tự nhiên ('Answer') giải thích lý do tại sao bạn đề xuất phòng đó.

Trích xuất thông tin của phòng đó vào 'ResponseDataAI'.

Loại bỏ dữ liệu nếu phát hiện trường IsDelete = true.
TrustScore của User càng cao càng tốt và được tính trên thang điểm 100.

QUAN TRỌNG: ĐỊNH DẠNG TRẢ LỜI (CHỈ TRẢ VỀ JSON): Bạn PHẢI trả lời bằng một chuỗi JSON duy nhất, không có bất kỳ văn bản nào khác, theo định dạng sau. (Trường Status phải là 'Available' nếu bạn chọn 1 phòng).

JSON

{outputFormatJson}
Hãy bắt đầu phân tích và trả về JSON. ";

            // 5. Gọi Gemini API
            string geminiResult = await CallGemini(prompt);

            // 6. Xử lý kết quả và Deserialize
            try
            {
                // Gemini đôi khi trả về Markdown, cần dọn dẹp
                if (geminiResult.StartsWith("```json"))
                {
                    geminiResult = geminiResult.Substring(7, geminiResult.Length - 10).Trim();
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true // Giúp khớp "answer" với "Answer"
                };
                var responseDto = JsonSerializer.Deserialize<ResponseSearchAIDto>(geminiResult, options);
                return responseDto ?? new ResponseSearchAIDto
                {
                    Answer = "Xin lỗi, tôi không thể phân tích phản hồi từ AI.",
                    ResponseDataAI = null
                };
            }
            catch (Exception ex)
            {
                // Nếu Gemini trả về không đúng JSON
                return new ResponseSearchAIDto
                {
                    Answer = $"Xin lỗi, tôi gặp lỗi khi xử lý phản hồi từ AI: {ex.Message}. Kết quả thô: {geminiResult}",
                    ResponseDataAI = null
                };
            }
        }


        // ==================================================================
        // HÀM HELPER: GỌI GEMINI API
        // (Lấy từ file GeminiService2.cs của bạn)
        // ==================================================================
        private async Task<string> CallGemini(string prompt)
        {
            var requestBody = new
            {
                contents = new[]
                {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            }
            }; //

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            ); //

            var requestUrl = $"{_geminiModel.Endpoint}?key={_geminiModel.ApiKey}"; //
            var response = await _httpClient.PostAsync(requestUrl, jsonContent); //

            if (!response.IsSuccessStatusCode) //
            {
                throw new Exception($"⚠️ Lỗi gọi Gemini API: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}"); //
            }

            var jsonResponse = await response.Content.ReadAsStringAsync(); //

            try
            {
                var jsonDocument = JsonDocument.Parse(jsonResponse); //
                var resultText = jsonDocument
                    .RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString(); //

                return resultText ?? "Xin lỗi!!Không có câu trả lời"; //
            }
            catch (Exception ex) //
            {
                throw new Exception($"⚠️ Lỗi parse JSON từ Gemini API: {ex.Message}\nResponse: {jsonResponse}"); //
            }
        }
    }
}