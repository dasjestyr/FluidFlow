using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FluidFlow.Serialization;
using FluidFlow.Tasks;
using Moq;
using Xunit;

namespace FluidFlow.Tests.Tasks
{
    [ExcludeFromCodeCoverage]
    public class WorkflowTests
    {
        private readonly Mock<ITaskStateStore> _store;
        private readonly Mock<IServiceQueue> _serviceMonitor;
        private readonly Workflow _workflow;

        public WorkflowTests()
        {
            _serviceMonitor = new Mock<IServiceQueue>();
            _store = new Mock<ITaskStateStore>();
            _workflow = new Workflow(_serviceMonitor.Object, _store.Object);
        }

        [Fact]
        public void Ctor_GeneratesNewId()
        {
            // arrange

            // act
            var wf1 = new Workflow(_serviceMonitor.Object, _store.Object);
            var wf2 = new Workflow(_serviceMonitor.Object, _store.Object);

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
            Assert.Throws<ArgumentNullException>(() => new Workflow(_serviceMonitor.Object, null));
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
            var last = _workflow.PendingActivities.LastOrDefault() as ParallelActivity;
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

            var last = _workflow.PendingActivities.LastOrDefault() as ParallelActivity;

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
            var addedTask = wf.PendingActivities.Any(t => t.Id == id);
            return addedTask;
        }
    }
}
