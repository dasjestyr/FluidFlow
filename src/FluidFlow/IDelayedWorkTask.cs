namespace FluidFlow
{
    public interface IDelayedWorkTask : IWorkTask
    {
        /// <summary>
        /// The state monitor.
        /// </summary>
        IStateMonitor StateMonitor { get; }
    }
}