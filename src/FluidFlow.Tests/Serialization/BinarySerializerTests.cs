using System.Diagnostics.CodeAnalysis;
using FluidFlow.Serialization;
using FluidFlow.Tests.Tasks;
using Xunit;

namespace FluidFlow.Tests.Serialization
{
    [ExcludeFromCodeCoverage]
    public class BinarySerializerTests
    {
        [Fact]
        public void Serialize_ReturnsBuffer()
        {
            // arrange
            var serializer = new BinarySerializer();
            var task = new FakeActivity();

            // act
            var blob = serializer.Serialize(task) as byte[];

            // assert
            Assert.True(blob.Length > 0);
        }

        [Fact]
        public void Deserialize_ReturnsObject()
        {
            // arrange
            var serializer = new BinarySerializer();
            var task = new FakeActivity();
            var blob = serializer.Serialize(task) as byte[];

            // act
            var obj = serializer.Deserialize<FakeActivity>(blob);

            // assert
            Assert.Equal(task.Id, obj.Id);
        }
    }
}
