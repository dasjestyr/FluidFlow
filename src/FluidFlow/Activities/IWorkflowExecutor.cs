using System;
using System.Threading.Tasks;

namespace FluidFlow.Activities
{
    public interface IWorkflowExecutor
    {
        /// <summary>
        /// Gets the service queue.
        /// </summary>
        /// <value>
        /// The service queue.
        /// </value>
        IServiceQueue ServiceQueue { get; }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">activity</exception>
        Task Execute();
    }
}