﻿using System;
using System.Threading.Tasks;

namespace FluidFlow.Tasks
{
    internal class WorkflowExecutor
    {
        private readonly IWorkflowActivity _parentActivity;
        private readonly IServiceQueue _serviceQueue;

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
            _serviceQueue = serviceQueue;
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
                case ActivityType.SychronizedTask:
                case ActivityType.Parallel:
                case ActivityType.Specification:
                    _parentActivity.State = ActivityState.Executing;
                    await activity.Run();
                    DequeueActivity();
                    break;
                case ActivityType.FireAndForget:
                    _parentActivity.State = ActivityState.Executing;
                    RunAndRemove(activity).Start(); // allow it to dequeue in the background
                    break;
                case ActivityType.Delayed:
                    _serviceQueue.AddTask(activity as IDelayedActivity);
                    _parentActivity.State = ActivityState.Delayed;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("activity", $"Unknown task type {activity.Type}");
            }
        }

        private async Task RunAndRemove(IActivity activity)
        {
            await activity.Run();
            DequeueActivity();
        }

        private void DequeueActivity()
        {
            _parentActivity.ActivityQueue.Dequeue();
        }
    }
}
