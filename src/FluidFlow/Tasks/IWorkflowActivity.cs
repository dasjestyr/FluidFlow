using System.Collections.Generic;

namespace FluidFlow.Tasks
{
    public interface IWorkflowActivity : IActivity
    {
        /// <summary>
        /// Gets the activity queue.
        /// </summary>
        /// <value>
        /// The activity queue.
        /// </value>
        Queue<IActivity> ActivityQueue { get; }
    }
}