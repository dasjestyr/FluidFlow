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
        Workflow Do(IActivity activity);

        /// <summary>
        /// Execute the task and then hand it off to a service queue to monitor state.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <returns></returns>
        Workflow WaitFor(IActivity activity);

        /// <summary>
        /// Execute the task and do not wait for it to finish.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <returns></returns>
        Workflow FireAndForget(IActivity activity);

        /// <summary>
        /// Runs the specified task at the same time as the previous task.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <returns></returns>
        Workflow And(IActivity activity);

        /// <summary>
        /// If the result of the previous activity satisifies the provided specification, run the provided activity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="specification">The specification.</param>
        /// <param name="onSuccess">The activity that will run if the specification is satisfied.</param>
        /// <param name="onFailure">Optional. Runs when the specification is not satisfied.</param>
        /// <returns></returns>
        Workflow Condition<T>(ISpecification<T> specification, IActivity onSuccess, IActivity onFailure = null);

        /// <summary>
        /// Saves the state.
        /// </summary>
        /// <returns></returns>
        Task SaveState();
    }
}