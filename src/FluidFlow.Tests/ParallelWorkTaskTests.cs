﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace FluidFlow.Tests
{
    [ExcludeFromCodeCoverage]
    public class ParallelWorkTaskTests
    {
        [Fact]
        public void Ctor_TasksNotNull()
        {
            // arrange
            var task = new ParallelWorkTask();

            // act
            var tasks = task.Tasks;

            // assert
            Assert.NotNull(tasks);
        }

        [Fact]
        public void Add_DuplicateTask_IsIgnored()
        {
            // arrange
            var task = new FakeWorkTask();
            var pTask = new ParallelWorkTask();

            // act
            pTask.Add(task);
            pTask.Add(task);

            // assert
            Assert.Equal(1, pTask.Tasks.Count);
        }

        [Fact]
        public void Add_NullTask_Throws()
        {
            // arrange
            var task = new ParallelWorkTask();

            // act
            Assert.Throws<ArgumentNullException>(() => task.Add(null));
        }

        [Fact]
        public void Add_ValidTask_Added()
        {
            // arrange
            var task1Mock = new FakeWorkTask();
            var task2Mock = new FakeWorkTask();
            var pTask = new ParallelWorkTask();

            // act
            pTask.Add(task1Mock);
            pTask.Add(task2Mock);

            // assert
            Assert.True(pTask.Tasks.Any(task => task.Id == task1Mock.Id), "Task 1 not found");
            Assert.True(pTask.Tasks.Any(task => task.Id == task2Mock.Id), "Task 2 not found");
        }

        [Fact]
        public async void Run_AlreadyStarted_Throws()
        {
            // arrange
            var pTask = new ParallelWorkTask();
            pTask.State = TaskState.Executing;

            // act

            // assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await pTask.Run());
        }

        [Fact]
        public async void Run_AllTasksRun()
        {
            // arrange
            var pTask = new ParallelWorkTask();

            var task1 = new Mock<IWorkTask>();
            task1.SetupAllProperties();
            task1.Setup(m => m.Run()).Returns(Task.FromResult(""));
            task1.SetupGet(m => m.Id).Returns(Guid.NewGuid());
            pTask.Add(task1.Object);

            var task2 = new Mock<IWorkTask>();
            task2.SetupAllProperties();
            task2.Setup(m => m.Run()).Returns(Task.FromResult("")); 
            task2.SetupGet(m => m.Id).Returns(Guid.NewGuid());
            pTask.Add(task2.Object);

            // act
            await pTask.Run();

            // assert
            task1.Verify(m => m.Run(), Times.Once, "Task 1 didn't run");
            task2.Verify(m => m.Run(), Times.Once, "Task 2 didn't run");
        }
    }

    public class FakeWorkTask : WorkTask
    {
        public override Task OnRun()
        {
            throw new NotImplementedException();
        }
    }
}
