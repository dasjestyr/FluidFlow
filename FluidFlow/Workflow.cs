using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluidFlow
{
    public class Workflow
    {
        private readonly IServiceQueue _stateMonitor;
        private readonly List<IWorkTask> _tasks = new List<IWorkTask>();
        private WorkflowState _workflowState = WorkflowState.NotStarted;

        /// <summary>
        /// The unique ID of this workflow.
        /// </summary>
        public Guid WorkflowId { get; private set; }

        /// <summary>
        /// A read-only collection of all tasks in this workflow.
        /// </summary>
        public IReadOnlyCollection<IWorkTask> Tasks => _tasks;

        public Workflow(IServiceQueue stateMonitor)
        {
            _stateMonitor = stateMonitor;
            WorkflowId = Guid.NewGuid();
        }

        public Workflow Do(IWorkTask task)
        {
            task.Type = TaskType.SychronizedTask;
            _tasks.Add(task);

            return this;
        }

        public Workflow WaitFor(IWorkTask task)
        {
            task.Type = TaskType.Delayed;
            _tasks.Add(task);

            return this;
        }

        public Workflow FireAndForget(IWorkTask task)
        {
            task.Type = TaskType.FireAndForget;
            _tasks.Add(task);

            return this;
        }

        public async Task Start()
        {
            _workflowState = WorkflowState.Executing;

            foreach (var task in _tasks)
            {
                if (_workflowState == WorkflowState.Delayed)
                {
                    SaveState();
                    break;
                }

                switch (task.Type)
                {
                    case TaskType.SychronizedTask:
                        _workflowState = WorkflowState.Executing;
                        await task.Run();
                        break;
                    case TaskType.FireAndForget:
                        _workflowState = WorkflowState.Executing;
                        task.Run().Start();
                        break;
                    case TaskType.Parallel:

                        break;
                    case TaskType.Delayed:
                        _stateMonitor.AddTask(task as IDelayedWorkTask);
                        _workflowState = WorkflowState.Delayed;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("task", $"Unknown task type {task.Type}");
                }
            }
        }

        private void SaveState()
        {
            
        }
    }

    public interface IStateMonitor
    {
        /// <summary>
        /// Current status of the task.
        /// </summary>
        TaskStatus TaskState { get; }

        /// <summary>
        /// Executes procedures necessary to update the status of a delayed task.
        /// </summary>
        /// <returns></returns>
        Task UpdateStatus();
    }

    public interface IDelayedWorkTask : IWorkTask
    {
        /// <summary>
        /// The state monitor.
        /// </summary>
        IStateMonitor StateMonitor { get; }
    }

    public interface IServiceQueue
    {
        /// <summary>
        /// Adds the task to the service queue.
        /// </summary>
        /// <param name="task"></param>
        void AddTask(IDelayedWorkTask task);
    }

    public interface IWorkTask
    {
        /// <summary>
        /// The unique id of this task.
        /// </summary>
        Guid TaskId { get; }

        /// <summary>
        /// The type of tasks this instance represents.
        /// </summary>
        TaskType Type { get; set; }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns></returns>
        Task Run();
    }
}
