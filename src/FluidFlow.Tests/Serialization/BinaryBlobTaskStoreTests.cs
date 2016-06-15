using System;
using FluidFlow.Serialization;
using FluidFlow.Tasks;
using Moq;
using Xunit;

namespace FluidFlow.Tests.Serialization
{
    public class BinaryBlobTaskStoreTests
    {
        [Fact]
        public async void Save_SerializerIsCalled()
        {
            // arrange
            var serializerMock = new Mock<ITaskSerializer>();
            serializerMock.Setup(m => m.Serialize(It.IsAny<IWorkTask>()));
            var store = new BinaryBlobTaskStore(serializerMock.Object);
            var task = new Mock<IWorkTask>();

            // act
            await store.Save(task.Object);

            // assert
            serializerMock.Verify(m => m.Serialize(It.IsAny<IWorkTask>()), Times.Once);
        }

        [Fact]
        public async void Save_DeserializeIsCalled()
        {
            // arrange
            var serializerMock = new Mock<ITaskSerializer>();
            serializerMock.Setup(m => m.Deserialize<IWorkTask>(It.IsAny<byte[]>()));
            var store = new BinaryBlobTaskStore(serializerMock.Object);
            
            // act
            await store.Get<IWorkTask>(Guid.NewGuid());

            // assert
            serializerMock.Verify(m => m.Deserialize<IWorkTask>(It.IsAny<byte[]>()), Times.Once);
        }
    }
}
