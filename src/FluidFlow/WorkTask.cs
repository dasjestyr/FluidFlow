using System;
using System.Threading.Tasks;

namespace FluidFlow
{
    public abstract class WorkTask : IWorkTask
    {
        /// <summary>
        /// Gets the state of the task.
        /// </summary>
        /// <value>
        /// The state of the task.
        /// </value>
        public TaskState State { get; internal set; }

        /// <summary>
        /// The unique id of this task.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// The type of tasks this instance represents.
        /// </summary>
        public TaskType Type { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkTask"/> class.
        /// </summary>
        protected WorkTask()
        {
            State = TaskState.NotStarted;
            Id = Guid.NewGuid();
        }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns></returns>
        public abstract Task OnRun();

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns></returns>
        public async Task Run()
        {
            if (State != TaskState.NotStarted)
                throw new InvalidOperationException("Cannot start a tasks that has already by started.");

            State = TaskState.Executing;
            await OnRun();
        }
    }
}