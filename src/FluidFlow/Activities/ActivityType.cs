namespace FluidFlow.Activities
{
    public enum ActivityType
    {
        /// <summary>
        /// This task must be completed before the workflow can continue.
        /// </summary>
        SychronizedTask,

        /// <summary>
        /// This task has no dependencies and the workflow can continue before this task is completed (e.g. sending a notification).
        /// </summary>
        FireAndForget,

        /// <summary>
        /// This task does not have dependent tasks and can be executed on a separate thread. Use this with other tasks when all tasks must completed before moving to the next step.
        /// </summary>
        Parallel,

        /// <summary>
        /// This activity only runs if the specification is satisfied.
        /// </summary>
        Specification,

        /// <summary>
        /// The task must be completed by an external system or person. Execution will be paused until the external task state has been changed to completed.
        /// </summary>
        Delayed
    }
}