namespace FluidFlow.Tasks
{
    public interface IServiceQueue
    {
        /// <summary>
        /// Adds the task to the service queue.
        /// </summary>
        /// <param name="task"></param>
        void AddTask(IDelayedActivity task);
    }
}