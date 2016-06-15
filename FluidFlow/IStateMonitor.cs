using System.Threading.Tasks;

namespace FluidFlow
{
    public interface IStateMonitor
    {
        /// <summary>
        /// Current status of the task.
        /// </summary>
        TaskStatus TaskState { get; }

        /// <summary>
        /// Executes procedures necessary to update the status of a delayed task.
        /// </summary>
        /// <returns></returns>
        Task UpdateStatus();
    }
}