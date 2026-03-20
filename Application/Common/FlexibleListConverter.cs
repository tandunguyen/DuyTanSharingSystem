using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.DTOs.Accommodation;

public class FlexibleListConverter : JsonConverter<List<GetAllAccommodationPostDto.LatLogAccommodation>>
{
    public override List<GetAllAccommodationPostDto.LatLogAccommodation>? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            // ✅ Trường hợp là mảng []
            return JsonSerializer.Deserialize<List<GetAllAccommodationPostDto.LatLogAccommodation>>(ref reader, options);
        }
        else if (reader.TokenType == JsonTokenType.StartObject)
        {
            // ✅ Trường hợp là object {}
            var single = JsonSerializer.Deserialize<GetAllAccommodationPostDto.LatLogAccommodation>(ref reader, options);
            return single != null ? new List<GetAllAccommodationPostDto.LatLogAccommodation> { single } : new List<GetAllAccommodationPostDto.LatLogAccommodation>();
        }
        else if (reader.TokenType == JsonTokenType.Null)
        {
            // ✅ Trường hợp null
            return new List<GetAllAccommodationPostDto.LatLogAccommodation>();
        }

        throw new JsonException($"Unexpected token {reader.TokenType} when parsing ResponseDataAI.");
    }

    public override void Write(Utf8JsonWriter writer, List<GetAllAccommodationPostDto.LatLogAccommodation> value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}
