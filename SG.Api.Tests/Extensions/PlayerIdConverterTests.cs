using System.Text.Json;
using SG.Api.Extensions;
using Xunit;

namespace SG.Api.Tests.Extensions
{
    public class PlayerIdConverterTests
    {
        private readonly JsonSerializerOptions _options;

        public PlayerIdConverterTests()
        {
            _options = new JsonSerializerOptions
            {
                Converters = { new PlayerIdConverter() },
                PropertyNameCaseInsensitive = true
            };
        }

        [Fact]
        public void Deserialize_ValidStringPlayerId_ReturnsString()
        {
            var json = "{\"PlayerId\":\"Player123\"}";
            var result = JsonSerializer.Deserialize<TestModel>(json, _options);

            Assert.IsType<string>(result.PlayerId);
            Assert.Equal("Player123", result.PlayerId);
        }

        [Fact]
        public void Deserialize_ValidIntPlayerId_ReturnsInt()
        {
            var json = "{\"PlayerId\":12345}";
            var result = JsonSerializer.Deserialize<TestModel>(json, _options);

            Assert.IsType<int>(result.PlayerId);
            Assert.Equal(12345, result.PlayerId);
        }

        [Fact]
        public void Deserialize_InvalidStringPlayerId_TooLong_ThrowsJsonException()
        {
            var json = "{\"PlayerId\":\"" + new string('A', 51) + "\"}";

            var exception = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<TestModel>(json, _options));
            Assert.Equal("PlayerId cannot exceed 50 characters.", exception.Message);
        }

        [Fact]
        public void Deserialize_InvalidStringPlayerId_ContainsInvalidCharacter_ThrowsJsonException()
        {
            var json = "{\"PlayerId\":\"Player~123\"}";

            var exception = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<TestModel>(json, _options));
            Assert.Equal("PlayerId cannot contain the '~' character.", exception.Message);
        }

        [Fact]
        public void Deserialize_InvalidPlayerIdType_ThrowsJsonException()
        {
            var json = "{\"PlayerId\":123.45}";

            var exception = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<TestModel>(json, _options));
            Assert.Equal("PlayerId must be a string or int.", exception.Message);
        }

        [Fact]
        public void Serialize_IntPlayerId_ReturnsJsonNumber()
        {
            var model = new TestModel { PlayerId = 12345 };
            var json = JsonSerializer.Serialize(model, _options);

            Assert.Equal("{\"PlayerId\":12345}", json);
        }

        [Fact]
        public void Serialize_StringPlayerId_ReturnsJsonString()
        {
            var model = new TestModel { PlayerId = "Player123" };
            var json = JsonSerializer.Serialize(model, _options);

            Assert.Equal("{\"PlayerId\":\"Player123\"}", json);
        }

        [Fact]
        public void Serialize_InvalidPlayerIdType_ThrowsJsonException()
        {
            var model = new TestModel { PlayerId = 123.45 }; // Bị lỗi vì PlayerId chỉ được phép là int hoặc string

            var exception = Assert.Throws<JsonException>(() => JsonSerializer.Serialize(model, _options));
            Assert.Equal("PlayerId must be a string or int.", exception.Message);
        }
    }

    public class TestModel
    {
        public object PlayerId { get; set; }
    }
}
