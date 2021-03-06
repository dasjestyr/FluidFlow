﻿using System;
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
            Assert.Throws<ArgumentNullException>(() => new WorkflowActivity(_serviceQueue.Object, _store.Object, (IWorkflowExecutor) null));
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
                .Also(task2);

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
                .Also(task2)
                .Also(task3);

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
            Assert.Throws<InvalidOperationException>(() => _workflowActivity.Also(task));
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
            var wf = new WorkflowActivity(_serviceQueue.Object, _store.Object);
            
            var specification = new Mock<ISpecification<string>>();
            specification.Setup(m => m.IsSatisfiedBy(It.IsAny<string>())).Returns(false);

            var activity1 = new Mock<IActivity>();
            activity1.Setup(m => m.Run()).Returns(Task.CompletedTask);
            activity1.SetupGet(m => m.Result).Returns("Activity 1");
            activity1.SetupGet(m => m.State).Returns(ActivityState.Completed);

            var activity2 = new Mock<IActivity>();
            activity2.Setup(m => m.Run()).Returns(Task.CompletedTask);
            activity2.SetupGet(m => m.Result).Returns("Activity 2");

            wf.Do(activity1.Object)
                .If(specification.Object)
                    .Do(activity2.Object)
                    .Also(activity2.Object)
                    .FireAndForget(activity2.Object)
                    .WaitFor(activity2.Object)
                    .EndIf()
                .Do(activity1.Object);

            // act
            await wf.Run();

            // assert
            activity2.Verify(m => m.Run(), Times.Never);
            activity1.Verify(m => m.Run(), Times.Exactly(2)); // wouldlikely be 3 if failed
        }

        [Fact]
        public async void If_SpecificationSatisfied_AllActivitiesRun()
        {
            // arrange
            var wf = new WorkflowActivity(_serviceQueue.Object, _store.Object);

            var specification = new Mock<ISpecification<int>>();
            specification.Setup(m => m.IsSatisfiedBy(It.IsAny<int>())).Returns(true);

            var activity1 = new Mock<IActivity>();
            activity1.Setup(m => m.Run()).Returns(Task.CompletedTask);
            activity1.SetupGet(m => m.Result).Returns(1);
            activity1.SetupGet(m => m.State).Returns(ActivityState.Completed);

            var activity2 = new Mock<IActivity>();
            activity2.Setup(m => m.Run()).Returns(Task.CompletedTask);
            activity2.SetupGet(m => m.Result).Returns(1);

            wf.Do(activity1.Object)
                .If(specification.Object)
                    .Do(activity2.Object)
                    .Also(activity2.Object)
                    .FireAndForget(activity2.Object)
                    .WaitFor(activity2.Object)
                    .EndIf()
                .Do(activity1.Object);

            // act
            await wf.Run();

            // assert
            activity2.Verify(m => m.Run(), Times.AtLeast(3)); // actually 4, but the fire and forget may not have registered yet
            activity1.Verify(m => m.Run(), Times.Exactly(2)); 
        }
        
        [Fact]
        public async void Else_FailedSpec_ActivitiesAreRun()
        {
            // arrange
            var wf = new WorkflowActivity(_serviceQueue.Object, _store.Object);

            var specification = new Mock<ISpecification<int>>();
            specification.Setup(m => m.IsSatisfiedBy(It.IsAny<int>())).Returns(false);

            var activity1 = new Mock<IActivity>();
            activity1.Setup(m => m.Run()).Returns(Task.CompletedTask);
            activity1.SetupGet(m => m.Result).Returns(1);
            activity1.SetupGet(m => m.State).Returns(ActivityState.Completed);

            var activity2 = new Mock<IActivity>();
            activity2.Setup(m => m.Run()).Returns(Task.CompletedTask);
            activity2.SetupGet(m => m.Result).Returns(1);

            var activity3 = new Mock<IActivity>();
            activity3.Setup(m => m.Run()).Returns(Task.CompletedTask);
            activity3.SetupGet(m => m.Result).Returns(1);

            wf.Do(activity1.Object)
                .If(specification.Object)
                    .Do(activity2.Object)
                .Else()
                    .Do(activity3.Object)
                    .Also(activity3.Object)
                    .FireAndForget(activity3.Object)
                    .WaitFor(activity3.Object)
                    .EndIf()
                .Do(activity1.Object);

            // act
            await wf.Run();

            // assert
            activity1.Verify(m => m.Run(), Times.Exactly(2)); 
            activity2.Verify(m => m.Run(), Times.Never);
            activity3.Verify(m => m.Run(), Times.AtLeast(3)); // actually 4, but the fire and forget may not have registered yet
        }

        [Fact]
        public void Else_NoIF_Throws()
        {
            // arrange
            var wf = new WorkflowActivity(_serviceQueue.Object, _store.Object);

            var specification = new Mock<ISpecification<int>>();
            specification.Setup(m => m.IsSatisfiedBy(It.IsAny<int>())).Returns(false);

            var activity1 = new Mock<IActivity>();
            activity1.Setup(m => m.Run()).Returns(Task.CompletedTask);
            activity1.SetupGet(m => m.Result).Returns(1);
            activity1.SetupGet(m => m.State).Returns(ActivityState.Completed);

            var activity2 = new Mock<IActivity>();
            activity2.Setup(m => m.Run()).Returns(Task.CompletedTask);
            activity2.SetupGet(m => m.Result).Returns(1);

            // act

            // assert
            Assert.Throws<InvalidOperationException>(() =>
                wf.Do(activity1.Object)
                    .Else()
                    .EndIf()
                .Do(activity1.Object));
        }

        [Fact]
        public async void OnRun_DelayedTask_DoesNotProgress()
        {
            // arrange
            var workflow = new WorkflowActivity(_serviceQueue.Object, _store.Object);

            var activity1 = new Mock<IActivity>();
            activity1.SetupAllProperties();
            activity1.Setup(m => m.Run()).Returns(Task.CompletedTask);

            var activity2 = new Mock<IActivity>();
            activity2.SetupAllProperties();
            activity2.Setup(m => m.Run()).Returns(Task.CompletedTask);

            workflow
                .WaitFor(activity1.Object)
                .Do(activity2.Object);

            // act
            await workflow.Run();

            // assert
            activity2.Verify(m => m.Run(), Times.Never);
            Assert.NotEqual(activity2.Object, workflow.LastActivity);
        }

        private static IActivity GetWorkTask()
        {
            var taskMock = new Mock<IActivity>();
            taskMock.SetupAllProperties();
            taskMock.Setup(m => m.Id).Returns(Guid.NewGuid());
            return taskMock.Object;
        }

        private static bool TaskIsFound(
            Func<IActivity, IWorkflowActivity> func,
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
