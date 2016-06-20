namespace FluidFlow.Tasks
{
    public enum ActivityState
    {
        /// <summary>
        /// The workflow has not started.
        /// </summary>
        NotStarted,

        /// <summary>
        /// The workflow currently has a task in progress.
        /// </summary>
        Executing,

        /// <summary>
        /// The workflow is paused until one or more tasks have been completed by an external source.
        /// </summary>
        Delayed,

        /// <summary>
        /// The workflow has been completed.
        /// </summary>
        Completed
    }
}