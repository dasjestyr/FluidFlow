using System;
using System.Threading.Tasks;

namespace FluidFlow.Activities
{
    [Serializable]
    public abstract class Activity : IActivity
    {
        /// <summary>
        /// Gets the state of the task.
        /// </summary>
        /// <value>
        /// The state of the task.
        /// </value>
        public ActivityState State { get; set; }

        /// <summary>
        /// The unique id of this task.
        /// </summary>
        public Guid Id { get; }
        
        /// <summary>
        /// The type of tasks this instance represents.
        /// </summary>
        public ActivityType Type { get; set; }

        /// <summary>
        /// Gets the result.
        /// </summary>
        /// <value>
        /// The result.
        /// </value>
        public object Result { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Activity"/> class.
        /// </summary>
        protected Activity()
        {
            State = ActivityState.NotStarted;
            Id = Guid.NewGuid();
        }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns></returns>
        protected abstract Task OnRun();

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns></returns>
        public virtual async Task Run()
        {
            if (State != ActivityState.NotStarted)
                throw new InvalidOperationException("Cannot start a task that has already been started");

            State = ActivityState.Executing;
            await OnRun();
        }
    }

    [Serializable]
    public class ExpressionActivity : Activity
    {
        private readonly Action _onRun;

        public ExpressionActivity(ActivityType type, Action onRun)
        {
            _onRun = onRun;
            Type = type;
        }

        protected override async Task OnRun()
        {
            await Task.Run(_onRun);
        }
    }
}