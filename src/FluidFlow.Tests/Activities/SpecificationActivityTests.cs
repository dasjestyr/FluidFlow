using System;
using System.Collections.Generic;
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
                null));
        }

        [Fact]
        public void Ctor_NullList_Throws()
        {
            // arrange
            List<IActivity> list = null;

            // act

            // assert
            Assert.Throws<ArgumentNullException>(() => new ParallelActivity(list));
        }

        [Fact]
        public void Ctor_EmptyList_Throws()
        {
            // arrange
            var list = new List<IActivity>();

            // act

            // assert
            Assert.Throws<ArgumentNullException>(() => new ParallelActivity(list));
        }

        [Fact]
        public void Ctor_NotNullList_Initializes()
        {
            // arrange
            var list = new List<IActivity> {_activityMock.Object, _activityMock.Object};

            // act
            var pActivity = new ParallelActivity(list);

            // assert
            Assert.Equal(2, pActivity.Tasks.Count);
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
                completedActivity.Object));
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
                completedActivity.Object);

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
                completedActivity.Object);

            // assert
            Assert.NotNull(activity);
        }

        [Fact]
        public async void OnRun_NoFailTask_NoExceptionThrown()
        {
            // arrange
            var spec = new Mock<ISpecification<int>>();
            spec.Setup(m => m.IsSatisfiedBy(It.IsAny<int>())).Returns(false);

            var completedActivity = new Mock<IActivity>();
            completedActivity.SetupGet(m => m.State).Returns(ActivityState.Completed);
            completedActivity.SetupGet(m => m.Result).Returns(1);

            var activity = new SpecificationActivity<int>(spec.Object, completedActivity.Object);

            // act
            await activity.Run();

            // assert
            // no assertion needed
        }

        [Fact]
        public async void OnRun_FailTaskSpecified_IsRun()
        {
            // arrange
            var spec = new Mock<ISpecification<int>>();
            spec.Setup(m => m.IsSatisfiedBy(It.IsAny<int>())).Returns(false);

            var completedActivity = new Mock<IActivity>();
            completedActivity.SetupGet(m => m.State).Returns(ActivityState.Completed);
            completedActivity.SetupGet(m => m.Result).Returns(1);

            var onFail = new Mock<IActivity>();
            onFail.Setup(m => m.Run()).Returns(Task.CompletedTask);

            var activity = new SpecificationActivity<int>(spec.Object, completedActivity.Object);
            activity.FailTask = onFail.Object;

            // act
            await activity.Run();

            // assert
            onFail.Verify(m => m.Run(), Times.Once);
        }
    }
}
