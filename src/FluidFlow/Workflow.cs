using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FluidFlow
{
    [Serializable]
    public class Workflow : IWorkTask
    {
        private readonly IServiceQueue _stateMonitor;
        private readonly List<IWorkTask> _pendingTasks = new List<IWorkTask>();
        
        /// <summary>
        /// The unique ID of this workflow.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Gets the state of the task.
        /// </summary>
        /// <value>
        /// The state of the task.
        /// </value>
        public TaskState State { get; private set; }

        /// <summary>
        /// Gets or sets the type of the task.
        /// </summary>
        /// <value>
        /// The type of the task.
        /// </value>
        public TaskType Type { get; set; }

        /// <summary>
        /// A read-only collection of all tasks in this workflow.
        /// </summary>
        public IReadOnlyCollection<IWorkTask> PendingTasks => _pendingTasks;

        /// <summary>
        /// Initializes an instance of <see cref="Workflow"/>
        /// </summary>
        /// <param name="stateMonitor"></param>
        public Workflow(IServiceQueue stateMonitor)
        {
            _stateMonitor = stateMonitor;
            Id = Guid.NewGuid();
        }

        /// <summary>
        /// Executes the task and waits for it to complete.
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public Workflow Do(IWorkTask task)
        {
            task.Type = TaskType.SychronizedTask;
            _pendingTasks.Add(task);

            return this;
        }

        /// <summary>
        /// Execute the task and then hand it off to a service queue to monitor state.
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public Workflow WaitFor(IWorkTask task)
        {
            task.Type = TaskType.Delayed;
            _pendingTasks.Add(task);

            return this;
        }

        /// <summary>
        /// Execute the task and do not wait for it to finish.
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public Workflow FireAndForget(IWorkTask task)
        {
            task.Type = TaskType.FireAndForget;
            _pendingTasks.Add(task);

            return this;
        }

        /// <summary>
        /// Runs the specified task at the same time as the previous task.
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public Workflow And(IWorkTask task)
        {
            task.Type = TaskType.Parallel;

            var lastTask = _pendingTasks.LastOrDefault();
            if(lastTask == null || lastTask.Type == TaskType.Delayed)
                throw new InvalidOperationException("Cannot add a parallel task to a delayed task or an empty workflow");

            var lastTaskIndex = _pendingTasks.IndexOf(lastTask);
            var asParallelTask = lastTask as ParallelWorkTask;

            // imake sure the last task is a parallel task and add this task to it
            if (asParallelTask == null)
            {
                var parallelCollection = new ParallelWorkTask();
                parallelCollection.Add(lastTask);
                parallelCollection.Add(task);

                _pendingTasks[lastTaskIndex] = parallelCollection;
            }
            else
            {
                asParallelTask.Add(task);
                _pendingTasks[lastTaskIndex] = asParallelTask;
            }

            return this;
        }

        public async Task Run()
        {
            if(State != TaskState.NotStarted)
                throw new InvalidOperationException("The workflow has already been started and/or has already finished.");

            State = TaskState.Executing;

            foreach (var task in _pendingTasks)
            {
                if (State == TaskState.Delayed)
                {
                    SaveState();
                    break;
                }

                switch (task.Type)
                {
                    case TaskType.SychronizedTask:
                    case TaskType.Parallel:
                        State = TaskState.Executing;
                        await task.Run();
                        break;
                    case TaskType.FireAndForget:
                        State = TaskState.Executing;
                        task.Run().Start();
                        break;
                    case TaskType.Delayed:
                        _stateMonitor.AddTask(task as IDelayedWorkTask);
                        State = TaskState.Delayed;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("task", $"Unknown task type {task.Type}");
                }

                if (task.State == TaskState.Completed)
                    _pendingTasks.Remove(task);
            }
        }

        private void SaveState()
        {
            
        }
    }
}
