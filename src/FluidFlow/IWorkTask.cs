using System;
using System.Threading.Tasks;

namespace FluidFlow
{
    public interface IWorkTask
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
        TaskState State { get; }

        /// <summary>
        /// The type of tasks this instance represents.
        /// </summary>
        TaskType Type { get; set; }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns></returns>
        Task Run();
    }
}