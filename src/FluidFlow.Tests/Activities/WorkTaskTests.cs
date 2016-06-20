using System.Diagnostics.CodeAnalysis;
using FluidFlow.Activities;
using Moq;
using Xunit;

namespace FluidFlow.Tests.Activities
{
    [ExcludeFromCodeCoverage]
    public class WorkTaskTests
    {
        [Fact]
        public void Ctor_NewIdGenerated()
        {
            // arrange
            var task1Mock = new Mock<Activity>();
            var task2Mock = new Mock<Activity>();

            // act

            // assert
            Assert.NotEqual(task1Mock.Object.Id, task2Mock.Object.Id);
        }

        [Fact]
        public void Ctor_CorrectStartingState()
        {
            // arrange

            // act
            var task = new Mock<Activity>();

            // assert
            Assert.Equal(ActivityState.NotStarted, task.Object.State);
        }
    }
}
