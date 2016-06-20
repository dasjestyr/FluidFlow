using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluidFlow.Activities;
using FluidFlow.Specification;
using Moq;
using Xunit;

namespace FluidFlow.Tests.Activities
{
    [ExcludeFromCodeCoverage]
    public class SpecificationActivityTests
    {
        private readonly Mock<ISpecification<object>> _specificationMock;
        private readonly Mock<IActivity> _activityMock;

        public SpecificationActivityTests()
        {
            _specificationMock = new Mock<ISpecification<object>>();
            _activityMock = new Mock<IActivity>();
        }

        [Fact]
        public void Ctor_NullSpecification_Throws()
        {
            // arrange

            // act

            // assert
            Assert.Throws<ArgumentNullException>(() => new SpecificationActivity<object>(
                null,
                _activityMock.Object,
                _activityMock.Object,
                _activityMock.Object));
        }

        [Fact]
        public void Ctor_NullCompletedActivity_Throws()
        {
            // arrange

            // act

            // assert
            Assert.Throws<ArgumentNullException>(() => new SpecificationActivity<object>(
                _specificationMock.Object,
                null,
                _activityMock.Object,
                _activityMock.Object));
        }

        [Fact]
        public void Ctor_NullOnSuccess_Throws()
        {
            // arrange

            // act

            // assert
            Assert.Throws<ArgumentNullException>(() => new SpecificationActivity<object>(
                _specificationMock.Object,
                _activityMock.Object,
                null,
                _activityMock.Object));
        }

        [Fact]
        public void Ctor_CompletedActivityResultBadState_Throws()
        {
            // arrange
            _activityMock.SetupGet(m => m.State).Returns(ActivityState.Executing);

            // act

            // assert
            Assert.Throws<InvalidOperationException>(() => new SpecificationActivity<object>(
                _specificationMock.Object,
                _activityMock.Object,
                _activityMock.Object,
                _activityMock.Object));
        }

        [Fact]
        public void Ctor_NullResult_Throws()
        {
            // arrange
            _activityMock.SetupGet(m => m.Result).Returns(null);

            // act

            // assert
            Assert.Throws<InvalidOperationException>(() => new SpecificationActivity<object>(
                _specificationMock.Object,
                _activityMock.Object,
                _activityMock.Object,
                _activityMock.Object));
        }

        [Fact]
        public void Ctor_ResultUnexpectedType_Throws()
        {
            // arrange
            var completedActivity = new Mock<IActivity>();
            completedActivity.SetupGet(m => m.Result).Returns("Hello");
            completedActivity.SetupGet(m => m.State).Returns(ActivityState.Completed);

            var specification = new Mock<ISpecification<int>>();

            // act

            // assert
            Assert.Throws<InvalidOperationException>(() => new SpecificationActivity<int>(
                specification.Object,
                completedActivity.Object,
                _activityMock.Object,
                _activityMock.Object));
        }

        [Fact]
        public void Ctor_ValidParameters_Initializes()
        {
            // arrange
            var completedActivity = new Mock<IActivity>();
            completedActivity.SetupGet(m => m.Result).Returns("Hello");
            completedActivity.SetupGet(m => m.State).Returns(ActivityState.Completed);

            var specification = new Mock<ISpecification<string>>();

            // act
            var activity = new SpecificationActivity<string>(
                specification.Object,
                completedActivity.Object,
                _activityMock.Object,
                _activityMock.Object);

            // assert
            Assert.NotNull(activity);
        }

        [Fact]
        public void Ctor_ValidParametersNoFailureActivity_Initializes()
        {
            // arrange
            var completedActivity = new Mock<IActivity>();
            completedActivity.SetupGet(m => m.Result).Returns("Hello");
            completedActivity.SetupGet(m => m.State).Returns(ActivityState.Completed);

            var specification = new Mock<ISpecification<string>>();

            // act
            var activity = new SpecificationActivity<string>(
                specification.Object,
                completedActivity.Object,
                _activityMock.Object);

            // assert
            Assert.NotNull(activity);
        }

        [Fact]
        public async void OnRun_FailureWithFailureActivity_ActivityIsRun()
        {
            // arrange
            var failureActivity = new Mock<IActivity>();
            failureActivity.Setup(m => m.Run()).Returns(Task.CompletedTask);
            failureActivity.SetupGet(m => m.State).Returns(ActivityState.NotStarted);

            var completedActivity = new Mock<IActivity>();
            completedActivity.SetupGet(m => m.Result).Returns("Hello World");
            completedActivity.SetupGet(m => m.State).Returns(ActivityState.Completed);

            var specification = new Mock<ISpecification<string>>();
            specification.Setup(m => m.IsSatisfiedBy(It.IsAny<string>())).Returns(false);

            var specActivity = new SpecificationActivity<string>(
                specification.Object,
                completedActivity.Object,
                _activityMock.Object,
                failureActivity.Object);

            // act
            await specActivity.Run();

            // assert
            failureActivity.Verify(m => m.Run(), Times.Once);
        }

        [Fact]
        public async void OnRun_FailureWithNullFailureTask_NoError()
        {
            // arrange
            var completedActivity = new Mock<IActivity>();
            completedActivity.SetupGet(m => m.Result).Returns("Hello World");
            completedActivity.SetupGet(m => m.State).Returns(ActivityState.Completed);

            var specification = new Mock<ISpecification<string>>();
            specification.Setup(m => m.IsSatisfiedBy(It.IsAny<string>())).Returns(false);

            var specActivity = new SpecificationActivity<string>(
                specification.Object,
                completedActivity.Object,
                _activityMock.Object);

            // act
            await specActivity.Run();

            // assert
            Assert.True(true);
        }

        [Fact]
        public async void OnRun_Success_SuccessTaskRun()
        {
            // arrange
            var successActivity = new Mock<IActivity>();
            successActivity.Setup(m => m.Run()).Returns(Task.CompletedTask);
            successActivity.SetupGet(m => m.State).Returns(ActivityState.NotStarted);

            var completedActivity = new Mock<IActivity>();
            completedActivity.SetupGet(m => m.Result).Returns("Hello World");
            completedActivity.SetupGet(m => m.State).Returns(ActivityState.Completed);
            
            var specification = new Mock<ISpecification<string>>();
            specification.Setup(m => m.IsSatisfiedBy(It.IsAny<string>())).Returns(true);

            var specActivity = new SpecificationActivity<string>(
                specification.Object,
                completedActivity.Object,
                successActivity.Object);

            // act
            await specActivity.Run();

            // assert
            successActivity.Verify(m => m.Run(), Times.Once);
        }
    }
}
