using JfYu.Redis.Serializer.Newtonsoft;
using JfYu.UnitTests.Models;
using Newtonsoft.Json;
using System.Text;

namespace JfYu.UnitTests.Redis
{
    [Collection("Redis")]
    public class NewtonsoftSerializerTests
    {
        private readonly NewtonsoftSerializer _serializer;

        public NewtonsoftSerializerTests()
        {
            var settings = new JsonSerializerSettings();
            _serializer = new NewtonsoftSerializer(settings);
        }

        [Fact]
        public void Serialize_NonNullObject_ReturnsByteArray()
        {
            var obj = new TestModelFaker().Generate();
            var result = _serializer.Serialize(obj);

            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void Serialize_NullObject_ReturnsEmptyByteArray()
        {
            var result = _serializer.Serialize<object>(null);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void Deserialize_ValidByteArray_ReturnsObject()
        {
            var obj = new TestModelFaker().Generate();
            var serialized = _serializer.Serialize(obj);
            var deserialized = _serializer.Deserialize<TestModel>(serialized);

            Assert.NotNull(deserialized);
            Assert.Equal(obj, deserialized);
        }

        [Fact]
        public void Deserialize_InvalidByteArray_ThrowsException()
        {
            var invalidData = Encoding.UTF8.GetBytes("invalid json");

            Assert.Throws<JsonReaderException>(() => _serializer.Deserialize<object>(invalidData));
        }
    }
}