using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluidFlow.Activities;
using Moq;
using Xunit;

namespace FluidFlow.Tests.Activities
{
    public class WorkflowExecutorTests
    {
        private readonly Mock<IWorkflowActivity> _parentActivity;
        private readonly Mock<IServiceQueue> _serviceQueue;

        public WorkflowExecutorTests()
        {
            _parentActivity = new Mock<IWorkflowActivity>();
            _serviceQueue = new Mock<IServiceQueue>();
        }

        [Fact]
        public void Ctor_NullParentActivity_Throws()
        {
            // arrange

            // act

            // assert
            Assert.Throws<ArgumentNullException>(() => new WorkflowExecutor(null, _serviceQueue.Object));
        }

        [Fact]
        public void Ctor_NullServiceQueue_Throws()
        {
            // arrange

            // act

            // assert
            Assert.Throws<ArgumentNullException>(() => new WorkflowExecutor(_parentActivity.Object, null));
        }

        [Fact]
        public async void Execute_SynchronousTask_TaskWasAwaited()
        {
            // arrange
            var i = 0;

            var synchTask = new Mock<IActivity>();
            synchTask.SetupGet(m => m.State).Returns(ActivityState.NotStarted);
            synchTask.SetupGet(m => m.Type).Returns(ActivityType.SychronizedTask);
            synchTask.Setup(m => m.Run())
                .Returns(Task.Run(() => i++));

            var q = GetActivityQueue(synchTask.Object);
            _parentActivity.SetupGet(m => m.ActivityQueue).Returns(q);

            var executor = new WorkflowExecutor(_parentActivity.Object, _serviceQueue.Object);

            // act
            await executor.Execute();

            // assert
            Assert.Equal(1, i);
        }

        [Fact]
        public async void Execute_SynchronousTask_TaskWasDequeued()
        {
            // arrange
            var i = 0;

            var synchTask = new Mock<IActivity>();
            synchTask.SetupGet(m => m.State).Returns(ActivityState.NotStarted);
            synchTask.SetupGet(m => m.Type).Returns(ActivityType.SychronizedTask);
            synchTask.Setup(m => m.Run())
                .Returns(Task.Run(() => i++));

            var q = GetActivityQueue(synchTask.Object);
            _parentActivity.SetupGet(m => m.ActivityQueue).Returns(q);
            
            var executor = new WorkflowExecutor(_parentActivity.Object, _serviceQueue.Object);

            // act
            await executor.Execute();

            // assert
            Assert.Equal(0, _parentActivity.Object.ActivityQueue.Count);
        }

        [Fact]
        public async void Execute_FireAndForget_DidNotWait()
        {
            // arrange
            var i = 0;

            var fireAndForgetTask = new Mock<IActivity>();
            fireAndForgetTask.SetupGet(m => m.State).Returns(ActivityState.NotStarted);
            fireAndForgetTask.SetupGet(m => m.Type).Returns(ActivityType.FireAndForget);
            fireAndForgetTask.Setup(m => m.Run())
                .Returns(Task.Run(async () =>
                {
                    await Task.Delay(1000);
                    i++;
                }));

            var q = GetActivityQueue(fireAndForgetTask.Object);
            _parentActivity.SetupGet(m => m.ActivityQueue).Returns(q);

            var executor = new WorkflowExecutor(_parentActivity.Object, _serviceQueue.Object);

            // act
            await executor.Execute();

            // assert
            Assert.Equal(0, i);
        }

        [Fact]
        public async void Execute_Delayed_AddsTaskToServiceQueue()
        {
            // arrange
            var delayedTask = new Mock<IDelayedActivity>();
            delayedTask.SetupGet(m => m.State).Returns(ActivityState.NotStarted);
            delayedTask.SetupGet(m => m.Type).Returns(ActivityType.Delayed);

            var q = GetActivityQueue(delayedTask.Object);
            _parentActivity.SetupGet(m => m.ActivityQueue).Returns(q);

            var serviceQueue = new Mock<IServiceQueue>();
            serviceQueue.Setup(m => m.AddTask(It.IsAny<IDelayedActivity>()));

            var executor = new WorkflowExecutor(_parentActivity.Object, serviceQueue.Object);

            // act
            await executor.Execute();

            // assert
            serviceQueue.Verify(m => m.AddTask(It.IsAny<IDelayedActivity>()), Times.Once);
        }

        

        private static Queue<IActivity> GetActivityQueue(IActivity activity)
        {
            var q = new Queue<IActivity>();
            q.Enqueue(activity);

            return q;
        }
    }
}
