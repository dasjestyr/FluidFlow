using System;
using System.Diagnostics.CodeAnalysis;
using FluidFlow.Activities;
using FluidFlow.Serialization;
using Moq;
using Xunit;

namespace FluidFlow.Tests.Serialization
{
    [ExcludeFromCodeCoverage]
    public class BinaryBlobTaskStoreTests
    {
        [Fact]
        public async void Save_SerializerIsCalled()
        {
            // arrange
            var serializerMock = new Mock<ITaskSerializer>();
            serializerMock.Setup(m => m.Serialize(It.IsAny<IActivity>()));
            var store = new BinaryBlobTaskStore(serializerMock.Object);
            var task = new Mock<IActivity>();

            // act
            await store.Save(task.Object);

            // assert
            serializerMock.Verify(m => m.Serialize(It.IsAny<IActivity>()), Times.Once);
        }

        [Fact]
        public async void Save_DeserializeIsCalled()
        {
            // arrange
            var serializerMock = new Mock<ITaskSerializer>();
            serializerMock.Setup(m => m.Deserialize<IActivity>(It.IsAny<byte[]>()));
            var store = new BinaryBlobTaskStore(serializerMock.Object);
            
            // act
            await store.Get<IActivity>(Guid.NewGuid());

            // assert
            serializerMock.Verify(m => m.Deserialize<IActivity>(It.IsAny<byte[]>()), Times.Once);
        }
    }
}
