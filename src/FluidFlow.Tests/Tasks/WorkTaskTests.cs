using FluidFlow.Tasks;
using Moq;
using Xunit;

namespace FluidFlow.Tests.Tasks
{
    public class WorkTaskTests
    {
        [Fact]
        public void Ctor_NewIdGenerated()
        {
            // arrange
            var task1Mock = new Mock<WorkTask>();
            var task2Mock = new Mock<WorkTask>();

            // act

            // assert
            Assert.NotEqual(task1Mock.Object.Id, task2Mock.Object.Id);
        }

        [Fact]
        public void Ctor_CorrectStartingState()
        {
            // arrange

            // act
            var task = new Mock<WorkTask>();

            // assert
            Assert.Equal(TaskState.NotStarted, task.Object.State);
        }
    }
}
