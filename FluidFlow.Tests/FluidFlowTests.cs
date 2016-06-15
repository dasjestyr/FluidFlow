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
        private Mock<IStateMonitor> _stateMonitorMock;

        public FluidFlowTests()
        {
            _stateMonitorMock = new Mock<IStateMonitor>();
        }

        [Fact]
        public void Ctor_GeneratesNewId()
        {
            // arrange

            // act
            var wf1 = new Workflow(_stateMonitorMock.Object);
            var wf2 = new Workflow(_stateMonitorMock.Object);

            // assert
            Assert.NotEqual(wf1.WorkflowId, wf2.WorkflowId);
        }

        [Fact]
        public void Do_SetsTaskType()
        {
            // arrange
            var wf = new Workflow(_stateMonitorMock.Object);
            var task = new Mock<IWorkTask>();
            task.Setup(m => m.Type);
            task.SetupAllProperties();

            // act
            wf.Do(task.Object);

            // assert
            Assert.Equal(TaskType.SychronizedTask, task.Object.Type);
        }

        [Fact]
        public void Do_TaskIsAdded()
        {
            // arrange
            var id = Guid.NewGuid();
            var wf = new Workflow(_stateMonitorMock.Object);
            var task = new Mock<IWorkTask>();
            task.Setup(m => m.TaskId).Returns(id);

            // act
            var addedTask = TaskIsFound(
                wf.Do, task.Object, wf, id);

            // assert
            Assert.True(addedTask);
        }

        [Fact]
        public void WaitFor_SetsTaskType()
        {
            // arrange
            var wf = new Workflow(_stateMonitorMock.Object);
            var taskMock = new Mock<IWorkTask>();
            taskMock.Setup(m => m.Type);
            taskMock.SetupAllProperties();
            var task = taskMock.Object;

            // act
            wf.WaitFor(task);

            // assert
            Assert.Equal(TaskType.Delayed, task.Type);
        }

        [Fact]
        public void WaitFor_TaskIsAdded()
        {
            // arrange
            var id = Guid.NewGuid();
            var wf = new Workflow(_stateMonitorMock.Object);
            var taskMock = new Mock<IWorkTask>();
            taskMock.Setup(m => m.TaskId).Returns(id);

            // act
            var taskAdded = TaskIsFound(
                wf.WaitFor, taskMock.Object, wf, id);

            // assert
            Assert.True(taskAdded);
        }

        [Fact]
        public void FireAndForget_SetsTaskType()
        {
            // arrange
            var wf = new Workflow(_stateMonitorMock.Object);
            var taskMock = new Mock<IWorkTask>();
            taskMock.Setup(m => m.Type);
            taskMock.SetupAllProperties();
            var task = taskMock.Object;

            // act
            wf.FireAndForget(task);

            // assert
            Assert.Equal(TaskType.FireAndForget, task.Type);
        }

        [Fact]
        public void FireAndForget_TaskIsAdded()
        {
            // arrange
            var id = Guid.NewGuid();
            var wf = new Workflow(_stateMonitorMock.Object);
            var taskMock = new Mock<IWorkTask>();
            taskMock.Setup(m => m.TaskId).Returns(id);

            // act
            var taskAdded = TaskIsFound(
                wf.FireAndForget, taskMock.Object, wf, id);

            // assert
            Assert.True(taskAdded);
        }

        private static bool TaskIsFound(
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
