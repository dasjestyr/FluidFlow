using System;
using System.Threading.Tasks;
#pragma warning disable 4014

namespace FluidFlow.Activities
{
    [Serializable]
    internal class WorkflowExecutor : IWorkflowExecutor
    {
        private readonly IWorkflowActivity _parentActivity;

        /// <summary>
        /// Gets the service queue.
        /// </summary>
        /// <value>
        /// The service queue.
        /// </value>
        public IServiceQueue ServiceQueue { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowExecutor"/> class.
        /// </summary>
        /// <param name="parentActivity">The parent activity.</param>
        /// <param name="serviceQueue">The service queue.</param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public WorkflowExecutor(IWorkflowActivity parentActivity, IServiceQueue serviceQueue)
        {
            if(parentActivity == null)
                throw new ArgumentNullException(nameof(parentActivity));

            if(serviceQueue == null)
                throw new ArgumentNullException(nameof(serviceQueue));
            
            _parentActivity = parentActivity;
            ServiceQueue = serviceQueue;
        }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">activity</exception>
        public async Task Execute()
        {
            var activity = _parentActivity.ActivityQueue.Peek();
            switch (activity.Type)
            {
                // activities should be awaited
                case ActivityType.SychronizedTask:
                case ActivityType.Parallel:
                case ActivityType.Specification:
                    _parentActivity.State = ActivityState.Executing;
                    await activity.Run();
                    _parentActivity.ActivityQueue.Dequeue();
                    break;
                // activities should be started but not awaited
                case ActivityType.FireAndForget:
                    _parentActivity.State = ActivityState.Executing;
                    Task.Run(() => activity.Run()); // allow it to dequeue in the background
                    _parentActivity.ActivityQueue.Dequeue();
                    break;
                // activities should be run and then monitored for state change
                case ActivityType.Delayed:
                    await activity.Run();
                    ServiceQueue.AddTask(activity as IDelayedActivity);
                    _parentActivity.State = ActivityState.Delayed;
                    _parentActivity.SaveState();
                    break;
                default:
                    throw new ArgumentOutOfRangeException("activity.Type", $"Unknown task type {activity.Type}");
            }
        }
    }
}
