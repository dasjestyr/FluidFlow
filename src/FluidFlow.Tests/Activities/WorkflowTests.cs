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
        private readonly WorkflowActivity _workflowActivity;

        public WorkflowTests()
        {
            _serviceQueue = new Mock<IServiceQueue>();
            _store = new Mock<ITaskStateStore>();
            _workflowActivity = new WorkflowActivity(_serviceQueue.Object, _store.Object);
        }

        [Fact]
        public void Ctor_GeneratesNewId()
        {
            // arrange

            // act
            var wf1 = new WorkflowActivity(_serviceQueue.Object, _store.Object);
            var wf2 = new WorkflowActivity(_serviceQueue.Object, _store.Object);

            // assert
            Assert.NotEqual(wf1.Id, wf2.Id);
        }

        [Fact]
        public void Ctor_NullServiceQueue_Throws()
        {
            // arrange

            // act

            // assert
            Assert.Throws<ArgumentNullException>(() => new WorkflowActivity(null, _store.Object));
        }

        [Fact]
        public void Ctor_NullStore_Throws()
        {
            // arrange

            // act

            // assert
            Assert.Throws<ArgumentNullException>(() => new WorkflowActivity(_serviceQueue.Object, null));
        }

        [Fact]
        public void Ctor_NullExecutor_Throws()
        {
            // arrange

            // act

            // assert
            Assert.Throws<ArgumentNullException>(() => new WorkflowActivity(_serviceQueue.Object, _store.Object, null));
        }

        [Fact]
        public void Ctor_WithCustomExecutor_Initializes()
        {
            // arrange
            var executor = new Mock<IWorkflowExecutor>();

            // act
            var wf = new WorkflowActivity(_serviceQueue.Object, _store.Object, executor.Object);

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
            _workflowActivity.Do(task);

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
                _workflowActivity.Do, task.Object, _workflowActivity, id);

            // assert
            Assert.True(addedTask);
        }

        [Fact]
        public void WaitFor_SetsTaskType()
        {
            // arrange
            var task = GetWorkTask();

            // act
            _workflowActivity.WaitFor(task);

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
                _workflowActivity.WaitFor, taskMock.Object, _workflowActivity, id);

            // assert
            Assert.True(taskAdded);
        }

        [Fact]
        public void FireAndForget_SetsTaskType()
        {
            // arrange
            var task = GetWorkTask();

            // act
            _workflowActivity.FireAndForget(task);

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
                _workflowActivity.FireAndForget, taskMock.Object, _workflowActivity, id);

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
            _workflowActivity
                .Do(task1)
                .And(task2);

            // assert
            var last = _workflowActivity.ActivityQueue.ToList().LastOrDefault() as ParallelActivity;
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
            _workflowActivity
                .Do(task1)
                .And(task2)
                .And(task3);

            var last = _workflowActivity.ActivityQueue.ToList().LastOrDefault() as ParallelActivity;

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
            Assert.Throws<InvalidOperationException>(() => _workflowActivity.And(task));
        }

        [Fact]
        public async void SaveState_StoreIsCalled()
        {
            // arrange
            var store = new Mock<ITaskStateStore>();
            store.Setup(m => m.Save(It.IsAny<IActivity>())).Returns(Task.CompletedTask);
            var workflow = new WorkflowActivity(_serviceQueue.Object, store.Object);
            
            // act
            await workflow.SaveState();

            // assert
            store.Verify(m => m.Save(It.IsAny<IActivity>()), Times.Once);
        }

        [Fact]
        public async void Run_EmptyQueue_Throws()
        {
            // arrange
            var workFlow = new WorkflowActivity(_serviceQueue.Object, _store.Object);

            // act
            
            // assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => workFlow.Run());
        }

        [Fact]
        public async void Run_DelayedActivity_CausesShortCircuit()
        {
            // arrange
            var executor = new Mock<IWorkflowExecutor>();
            executor.Setup(m => m.Execute()).Returns(Task.CompletedTask);

            var activity1 = new Mock<IActivity>();
            activity1.SetupGet(m => m.Type).Returns(ActivityType.SychronizedTask);
            activity1.SetupGet(m => m.State).Returns(ActivityState.NotStarted);
            activity1.Setup(m => m.Run()).Returns(Task.CompletedTask);

            var workflow = new WorkflowActivity(_serviceQueue.Object, _store.Object);
            workflow.ActivityQueue = GetQueue(activity1.Object);
            workflow.State = ActivityState.Delayed;
            workflow.Do(activity1.Object);

            // TODO: figure out how to test this

            // act
            // call this instead of Run because we've overridden the state
            //await workflow.Run();

            // assert
            //executor.Verify(m => m.Execute(), Times.Never);
        }

        public Queue<IActivity> GetQueue(params IActivity[] activities)
        {
            return new Queue<IActivity>(activities);
        }

        [Fact]
        public async void Run_ActivityExecuted_IsUpdated()
        {
            // arrange
            var act1 = new Mock<IActivity>();
            act1.Setup(m => m.Run()).Returns(Task.CompletedTask);

            var wf = new WorkflowActivity(_serviceQueue.Object, _store.Object);
            wf.ActivityQueue = GetQueue(act1.Object, act1.Object);

            // act
            await wf.Run();

            // assert
            Assert.True(wf.LastActivity.Equals(act1.Object));
        }

        [Fact]
        public async void If_SpecificationFail_ConditionalNotRun()
        {
            // arrange
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var wf = new WorkflowActivity(_serviceQueue.Object, _store.Object);
            
            var specification = new Mock<ISpecification<int>>();
            specification.Setup(m => m.IsSatisfiedBy(It.IsAny<int>())).Returns(false);

            var activity1 = new Mock<IActivity>();
            activity1.Setup(m => m.Run()).Returns(Task.CompletedTask);
            activity1.SetupGet(m => m.Id).Returns(id1);
            activity1.SetupGet(m => m.Result).Returns(1);
            activity1.SetupGet(m => m.State).Returns(ActivityState.Completed);

            var activity2 = new Mock<IActivity>();
            activity2.SetupGet(m => m.Id).Returns(id2);
            activity2.Setup(m => m.Run()).Returns(Task.CompletedTask);
            activity2.SetupGet(m => m.Result).Returns(1);

            wf.Do(activity1.Object)
                .If(specification.Object)
                    .Do(activity1.Object)
                    .Do(activity2.Object)
                    .EndIf()
                .Do(activity1.Object);

            // act
            await wf.Run();

            // assert
            activity2.Verify(m => m.Run(), Times.Never);
            activity1.Verify(m => m.Run(), Times.Exactly(2)); // wouldlikely be 3 if failed
        }

        [Fact]
        public void If_AlreadyBuildingState_Throws()
        {
            // arrange
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var wf = new WorkflowActivity(_serviceQueue.Object, _store.Object);

            var specification = new Mock<ISpecification<int>>();
            specification.Setup(m => m.IsSatisfiedBy(It.IsAny<int>())).Returns(false);

            var activity1 = new Mock<IActivity>();
            activity1.Setup(m => m.Run()).Returns(Task.CompletedTask);
            activity1.SetupGet(m => m.Id).Returns(id1);
            activity1.SetupGet(m => m.Result).Returns(1);
            activity1.SetupGet(m => m.State).Returns(ActivityState.Completed);

            var activity2 = new Mock<IActivity>();
            activity2.SetupGet(m => m.Id).Returns(id2);
            activity2.Setup(m => m.Run()).Returns(Task.CompletedTask);
            activity2.SetupGet(m => m.Result).Returns(1);

            // act

            // assert
            Assert.Throws<InvalidOperationException>(() => 
                wf.Do(activity1.Object)
                    .If(specification.Object)
                        .Do(activity1.Object)
                        .Do(activity2.Object)
                    .If(specification.Object)
                    .EndIf()
                .Do(activity1.Object));
        }

        private static IActivity GetWorkTask()
        {
            var taskMock = new Mock<IActivity>();
            taskMock.SetupAllProperties();
            taskMock.Setup(m => m.Id).Returns(Guid.NewGuid());
            return taskMock.Object;
        }

        private static bool TaskIsFound(
            Func<IActivity, WorkflowActivity> func,
            IActivity task,
            WorkflowActivity wf,
            Guid id)
        {
            func(task);
            var addedTask = wf.ActivityQueue.ToList().Any(t => t.Id == id);
            return addedTask;
        }
    }
}
