using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Moq;
using Xunit;

namespace FluidFlow.Tests
{
    [ExcludeFromCodeCoverage]
    public class FluidFlowTests
    {
        private readonly Mock<IServiceQueue> _serviceMonitor;
        private readonly Workflow _workflow;

        public FluidFlowTests()
        {
            _serviceMonitor = new Mock<IServiceQueue>();
            _workflow = new Workflow(_serviceMonitor.Object);
        }

        [Fact]
        public void Ctor_GeneratesNewId()
        {
            // arrange

            // act
            var wf1 = new Workflow(_serviceMonitor.Object);
            var wf2 = new Workflow(_serviceMonitor.Object);

            // assert
            Assert.NotEqual(wf1.WorkflowId, wf2.WorkflowId);
        }

        [Fact]
        public void Do_SetsTaskType()
        {
            // arrange
            var task = GetWorkTask();

            // act
            _workflow.Do(task);

            // assert
            Assert.Equal(TaskType.SychronizedTask, task.Type);
        }

        [Fact]
        public void Do_TaskIsAdded()
        {
            // arrange
            var id = Guid.NewGuid();
            var task = new Mock<IWorkTask>();
            task.Setup(m => m.TaskId).Returns(id);

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
            Assert.Equal(TaskType.Delayed, task.Type);
        }

        [Fact]
        public void WaitFor_TaskIsAdded()
        {
            // arrange
            var id = Guid.NewGuid();
            var taskMock = new Mock<IWorkTask>();
            taskMock.Setup(m => m.TaskId).Returns(id);

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
            Assert.Equal(TaskType.FireAndForget, task.Type);
        }

        [Fact]
        public void FireAndForget_TaskIsAdded()
        {
            // arrange
            var id = Guid.NewGuid();
            var taskMock = new Mock<IWorkTask>();
            taskMock.Setup(m => m.TaskId).Returns(id);

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
            var last = _workflow.Tasks.LastOrDefault() as ParallelWorkTask;
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

            var last = _workflow.Tasks.LastOrDefault() as ParallelWorkTask;

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

        private static IWorkTask GetWorkTask()
        {
            var taskMock = new Mock<IWorkTask>();
            taskMock.SetupAllProperties();
            taskMock.Setup(m => m.TaskId).Returns(Guid.NewGuid());
            return taskMock.Object;
        }

        private bool TaskIsFound(
            Func<IWorkTask, Workflow> func,
            IWorkTask task,
            Workflow wf,
            Guid id)
        {
            func(task);
            var addedTask = wf.Tasks.Any(t => t.TaskId == id);
            return addedTask;
        }
    }
}
