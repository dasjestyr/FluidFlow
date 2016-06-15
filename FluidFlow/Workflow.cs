using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// Initializes an instance of <see cref="Workflow"/>
        /// </summary>
        /// <param name="stateMonitor"></param>
        public Workflow(IServiceQueue stateMonitor)
        {
            _stateMonitor = stateMonitor;
            WorkflowId = Guid.NewGuid();
        }

        /// <summary>
        /// Executes the task and waits for it to complete.
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public Workflow Do(IWorkTask task)
        {
            task.Type = TaskType.SychronizedTask;
            _tasks.Add(task);

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
            _tasks.Add(task);

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
            _tasks.Add(task);

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

            var lastTask = _tasks.LastOrDefault();
            if(lastTask == null || lastTask.Type == TaskType.Delayed)
                throw new InvalidOperationException("Cannot add a parallel task to a delayed task or an empty workflow");

            var lastTaskIndex = _tasks.IndexOf(lastTask);
            var asParallelTask = lastTask as ParallelWorkTask;

            // imake sure the last task is a parallel task and add this task to it
            if (asParallelTask == null)
            {
                var parallelCollection = new ParallelWorkTask();
                parallelCollection.Add(lastTask);
                parallelCollection.Add(task);

                _tasks[lastTaskIndex] = parallelCollection;
            }
            else
            {
                asParallelTask.Add(task);
                _tasks[lastTaskIndex] = asParallelTask;
            }

            return this;
        }

        public async Task Start()
        {
            if(_workflowState != WorkflowState.NotStarted)
                throw new InvalidOperationException("The workflow has already been started and/or has already finished.");

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
                    case TaskType.Parallel:
                        _workflowState = WorkflowState.Executing;
                        await task.Run();
                        break;
                    case TaskType.FireAndForget:
                        _workflowState = WorkflowState.Executing;
                        task.Run().Start();
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

    public class WorkTask : IWorkTask
    {
        public Guid TaskId { get; }

        public TaskType Type { get; set; }

        public WorkTask()
        {
            TaskId = Guid.NewGuid();
        }

        public virtual Task Run()
        {
            throw new NotImplementedException();
        }
    }
}
