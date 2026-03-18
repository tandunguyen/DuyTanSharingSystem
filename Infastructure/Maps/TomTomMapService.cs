using Application.Interface.Api;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Maps
{
    // Đổi tên class từ HereMapService thành TomTomMapService
    public class TomTomMapService : IMapService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public TomTomMapService(HttpClient httpClient, IOptions<MapsKeyModel> apiKey)
        {
            _httpClient = httpClient;
            _apiKey = apiKey.Value.ApiKey;
        }

        // 1. Geocoding: Tìm Tọa Độ từ Địa Chỉ
        public async Task<(double lat, double lng)> GetCoordinatesAsync(string address)
        {
            var value = Uri.EscapeDataString(address);
            // 🌐 Sử dụng TomTom Search API (Geocoding)
            var url = $"https://api.tomtom.com/search/2/geocode/{value}.json?key={_apiKey}";

            var response = await _httpClient.GetStringAsync(url);
            using var jsonDoc = JsonDocument.Parse(response);

            // 🛑 TomTom trả về "results"
            var results = jsonDoc.RootElement.GetProperty("results");
            if (results.GetArrayLength() == 0)
            {
                // Thay thế kiểm tra lỗi 'response.Contains("error")' bằng kiểm tra kết quả rỗng
                throw new Exception($"Invalid address: {address}");
            }

            // 📍 Vị trí tọa độ được lưu trong trường "position"
            var position = results[0].GetProperty("position");
            double lat = position.GetProperty("lat").GetDouble();
            // TomTom dùng "lon" thay vì "lng"
            double lng = position.GetProperty("lon").GetDouble();
            return (lat, lng);
        }

        // 2. Reverse Geocoding: Tìm Địa Chỉ từ Tọa Độ
        public async Task<string?> GetAddressFromCoordinatesAsync(double lat, double lng)
        {
            // 🌐 Sử dụng TomTom Reverse Geocoding API
            var url = $"https://api.tomtom.com/search/2/reverseGeocode/{lat},{lng}.json?key={_apiKey}";

            var response = await _httpClient.GetStringAsync(url);
            using var jsonDoc = JsonDocument.Parse(response);

            // 🏠 TomTom trả về "addresses" thay vì "items"
            var addresses = jsonDoc.RootElement.GetProperty("addresses");
            if (addresses.GetArrayLength() == 0)
            {
                return null;
            }

            // 📜 Lấy địa chỉ đầy đủ từ trường "freeformAddress"
            return addresses[0].GetProperty("address").GetProperty("freeformAddress").GetString();
        }

        // 3. Routing: Tính Khoảng Cách và Thời Gian
        public async Task<(double distanceKm, int durationMinutes)> GetDistanceAndTimeAsync(string origin, string destination)
        {
            // 🗺️ origin và destination dạng "lat,lon"
            // Ví dụ: origin = "20.863796,106.705372", destination = "16.0489397,108.216702"

            var url = $"https://api.tomtom.com/routing/1/calculateRoute/{origin}:{destination}/json?key={_apiKey}";

            var response = await _httpClient.GetStringAsync(url);
            using var jsonDoc = JsonDocument.Parse(response);

            if (!jsonDoc.RootElement.TryGetProperty("routes", out var routes) || routes.GetArrayLength() == 0)
            {
                throw new Exception($"Không tìm thấy tuyến đường giữa {origin} và {destination}");
            }

            var summary = routes[0].GetProperty("summary");

            double distanceKm = summary.GetProperty("lengthInMeters").GetDouble() / 1000.0;
            int durationMinutes = summary.GetProperty("travelTimeInSeconds").GetInt32() / 60;

            return (distanceKm, durationMinutes);
        }

    }
}