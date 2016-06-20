using System;
using System.Threading.Tasks;

namespace FluidFlow.Tasks
{
    public interface IActivity
    {
        /// <summary>
        /// The unique id of this task.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Gets the state of the task.
        /// </summary>
        /// <value>
        /// The state of the task.
        /// </value>
        ActivityState State { get; set; }

        /// <summary>
        /// The type of tasks this instance represents.
        /// </summary>
        ActivityType Type { get; set; }

        /// <summary>
        /// Gets the result.
        /// </summary>
        /// <value>
        /// The result.
        /// </value>
        object Result { get; }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns></returns>
        Task Run();
    }
}