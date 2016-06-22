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
        Queue<IActivity> ActivityQueue { get; set; }

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
        IWorkflowActivity Do(IActivity activity);

        /// <summary>
        /// Execute the task and then hand it off to a service queue to monitor state.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <returns></returns>
        IWorkflowActivity WaitFor(IActivity activity);

        /// <summary>
        /// Execute the task and do not wait for it to finish.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <returns></returns>
        IWorkflowActivity FireAndForget(IActivity activity);

        /// <summary>
        /// Runs the specified task at the same time as the previous task.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <returns></returns>
        IWorkflowActivity Also(IActivity activity);

        /// <summary>
        /// If the result of the previous activity satisifies the provided specification, run the provided activity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="specification">The specification.</param>
        /// <returns></returns>
        IWorkflowActivity If<T>(ISpecification<T> specification);

        /// <summary>
        /// Terminates the previous IF branch
        /// </summary>
        /// <returns></returns>
        IWorkflowActivity EndIf();

        /// <summary>
        /// Creates a branch that will be run when the previous specification was not satisfied
        /// </summary>
        /// <returns></returns>
        IWorkflowActivity Else();

        /// <summary>
        /// Saves the state.
        /// </summary>
        /// <returns></returns>
        Task SaveState();
    }
}