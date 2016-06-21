using System.Collections.Generic;
using System.Threading.Tasks;
using FluidFlow.Specification;

namespace FluidFlow.Activities
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

        /// <summary>
        /// Gets the last activity.
        /// </summary>
        /// <value>
        /// The last activity.
        /// </value>
        IActivity LastActivity { get; }

        /// <summary>
        /// Executes the task and waits for it to complete.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <returns></returns>
        WorkflowActivity Do(IActivity activity);

        /// <summary>
        /// Execute the task and then hand it off to a service queue to monitor state.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <returns></returns>
        WorkflowActivity WaitFor(IActivity activity);

        /// <summary>
        /// Execute the task and do not wait for it to finish.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <returns></returns>
        WorkflowActivity FireAndForget(IActivity activity);

        /// <summary>
        /// Runs the specified task at the same time as the previous task.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <returns></returns>
        WorkflowActivity And(IActivity activity);

        /// <summary>
        /// If the result of the previous activity satisifies the provided specification, run the provided activity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="specification">The specification.</param>
        /// <returns></returns>
        WorkflowActivity If<T>(ISpecification<T> specification);

        /// <summary>
        /// Saves the state.
        /// </summary>
        /// <returns></returns>
        Task SaveState();
    }
}