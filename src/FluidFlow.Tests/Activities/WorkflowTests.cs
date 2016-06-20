using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using FluidFlow.Activities;
using FluidFlow.Serialization;
using FluidFlow.Specification;
using Moq;
using Xunit;

namespace FluidFlow.Tests.Activities
{
    [ExcludeFromCodeCoverage]
    public class WorkflowTests
    {
        private readonly Mock<ITaskStateStore> _store;
        private readonly Mock<IServiceQueue> _serviceQueue;
        private readonly Workflow _workflow;

        public WorkflowTests()
        {
            _serviceQueue = new Mock<IServiceQueue>();
            _store = new Mock<ITaskStateStore>();
            _workflow = new Workflow(_serviceQueue.Object, _store.Object);
        }

        [Fact]
        public void Ctor_GeneratesNewId()
        {
            // arrange

            // act
            var wf1 = new Workflow(_serviceQueue.Object, _store.Object);
            var wf2 = new Workflow(_serviceQueue.Object, _store.Object);

            // assert
            Assert.NotEqual(wf1.Id, wf2.Id);
        }

        [Fact]
        public void Ctor_NullServiceQueue_Throws()
        {
            // arrange

            // act

            // assert
            Assert.Throws<ArgumentNullException>(() => new Workflow(null, _store.Object));
        }

        [Fact]
        public void Ctor_NullStore_Throws()
        {
            // arrange

            // act

            // assert
            Assert.Throws<ArgumentNullException>(() => new Workflow(_serviceQueue.Object, null));
        }

        [Fact]
        public void Ctor_NullExecutor_Throws()
        {
            // arrange

            // act

            // assert
            Assert.Throws<ArgumentNullException>(() => new Workflow(_serviceQueue.Object, _store.Object, null));
        }

        [Fact]
        public void Ctor_WithCustomExecutor_Initializes()
        {
            // arrange
            var executor = new Mock<IWorkflowExecutor>();

            // act
            var wf = new Workflow(_serviceQueue.Object, _store.Object, executor.Object);

            // assert
            Assert.NotNull(wf);
            Assert.True(true);
        }

        [Fact]
        public void Do_SetsTaskType()
        {
            // arrange
            var task = GetWorkTask();

            // act
            _workflow.Do(task);

            // assert
            Assert.Equal(ActivityType.SychronizedTask, task.Type);
        }

        [Fact]
        public void Do_TaskIsAdded()
        {
            // arrange
            var id = Guid.NewGuid();
            var task = new Mock<IActivity>();
            task.Setup(m => m.Id).Returns(id);

            // act
            var addedTask = TaskIsFound(
                _workflow.Do, task.Object, _workflow, id);

            // assert
            Assert.True(addedTask);
        }

        [Fact]
        public void WaitFor_SetsTaskType()
        {
            // arrange
            var task = GetWorkTask();

            // act
            _workflow.WaitFor(task);

            // assert
            Assert.Equal(ActivityType.Delayed, task.Type);
        }

        [Fact]
        public void WaitFor_TaskIsAdded()
        {
            // arrange
            var id = Guid.NewGuid();
            var taskMock = new Mock<IActivity>();
            taskMock.Setup(m => m.Id).Returns(id);

            // act
            var taskAdded = TaskIsFound(
                _workflow.WaitFor, taskMock.Object, _workflow, id);

            // assert
            Assert.True(taskAdded);
        }

        [Fact]
        public void FireAndForget_SetsTaskType()
        {
            // arrange
            var task = GetWorkTask();

            // act
            _workflow.FireAndForget(task);

            // assert
            Assert.Equal(ActivityType.FireAndForget, task.Type);
        }

        [Fact]
        public void FireAndForget_TaskIsAdded()
        {
            // arrange
            var id = Guid.NewGuid();
            var taskMock = new Mock<IActivity>();
            taskMock.Setup(m => m.Id).Returns(id);

            // act
            var taskAdded = TaskIsFound(
                _workflow.FireAndForget, taskMock.Object, _workflow, id);

            // assert
            Assert.True(taskAdded);
        }

        [Fact]
        public void And_ConvertsLastTaskToParallel()
        {
            // arrange
            var task1 = GetWorkTask();
            var task2 = GetWorkTask();

            // act
            _workflow
                .Do(task1)
                .And(task2);

            // assert
            var last = _workflow.ActivityQueue.ToList().LastOrDefault() as ParallelActivity;
            Assert.NotNull(last);
        }

        [Fact]
        public void And_AllTasksInCollection()
        {
            // arrange
            var task1 = GetWorkTask();
            var task2 = GetWorkTask();
            var task3 = GetWorkTask();

            // act
            _workflow
                .Do(task1)
                .And(task2)
                .And(task3);

            var last = _workflow.ActivityQueue.ToList().LastOrDefault() as ParallelActivity;

            // assert
            Assert.NotNull(last);
            Assert.Equal(3, last.Tasks.Count);
        }

        [Fact]
        public void And_EmptyWorkflow_Throws()
        {
            // arrange
            var task = GetWorkTask();

            // act

            // assert
            Assert.Throws<InvalidOperationException>(() => _workflow.And(task));
        }

        [Fact]
        public void Condition_ValidSpecification_IsAdded()
        {
            // arrange
            var spec = new Mock<ISpecification<int>>();
            var activity = new Mock<IActivity>();
            activity.Setup(m => m.State).Returns(ActivityState.Completed);
            activity.SetupGet(m => m.Result).Returns(1);

            var workflow = new Workflow(_serviceQueue.Object, _store.Object);
            workflow.LastActivity = activity.Object;
            
            // act
            workflow.Condition(spec.Object, activity.Object);

            // assert
            Assert.True(workflow.ActivityQueue.Last() is SpecificationActivity<int>);
        }

        [Fact]
        public async void SaveState_StoreIsCalled()
        {
            // arrange
            var store = new Mock<ITaskStateStore>();
            store.Setup(m => m.Save(It.IsAny<IActivity>())).Returns(Task.CompletedTask);
            var workflow = new Workflow(_serviceQueue.Object, store.Object);
            
            // act
            await workflow.SaveState();

            // assert
            store.Verify(m => m.Save(It.IsAny<IActivity>()), Times.Once);
        }

        [Fact]
        public async void OnRun_EmptyQueue_Throws()
        {
            // arrange
            var workFlow = new Workflow(_serviceQueue.Object, _store.Object);

            // act
            
            // assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => workFlow.OnRun());
        }

        [Fact]
        public async void OnRun_DelayedActivity_CausesShortCircuit()
        {
            // arrange
            var executor = new Mock<IWorkflowExecutor>();
            executor.Setup(m => m.Execute()).Returns(Task.CompletedTask);

            var activity1 = new Mock<IActivity>();
            activity1.SetupGet(m => m.Type).Returns(ActivityType.SychronizedTask);
            activity1.SetupGet(m => m.State).Returns(ActivityState.NotStarted);
            activity1.Setup(m => m.Run()).Returns(Task.CompletedTask);

            var workflow = new Workflow(_serviceQueue.Object, _store.Object);
            workflow.ActivityQueue = GetQueue(activity1.Object);
            workflow.State = ActivityState.Delayed;
            workflow.Do(activity1.Object);

            // act
            // call this instead of Run because we've overridden the state
            await workflow.OnRun();

            // assert
            executor.Verify(m => m.Execute(), Times.Never);
        }

        public Queue<IActivity> GetQueue(params IActivity[] activities)
        {
            return new Queue<IActivity>(activities);
        }

        [Fact]
        public async void OnRun_ActivityExecuted_IsUpdated()
        {
            // arrange
            var act1 = new Mock<IActivity>();
            act1.Setup(m => m.Run()).Returns(Task.CompletedTask);

            var wf = new Workflow(_serviceQueue.Object, _store.Object);
            wf.ActivityQueue = GetQueue(act1.Object, act1.Object);

            // act
            await wf.OnRun();

            // assert
            Assert.True(wf.LastActivity.Equals(act1.Object));
        }

        private static IActivity GetWorkTask()
        {
            var taskMock = new Mock<IActivity>();
            taskMock.SetupAllProperties();
            taskMock.Setup(m => m.Id).Returns(Guid.NewGuid());
            return taskMock.Object;
        }

        private static bool TaskIsFound(
            Func<IActivity, Workflow> func,
            IActivity task,
            Workflow wf,
            Guid id)
        {
            func(task);
            var addedTask = wf.ActivityQueue.ToList().Any(t => t.Id == id);
            return addedTask;
        }
    }
}
