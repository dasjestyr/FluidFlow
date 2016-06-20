namespace FluidFlow.Tasks
{
    public interface IDelayedActivity : IActivity
    {
        /// <summary>
        /// The state monitor.
        /// </summary>
        IStateMonitor StateMonitor { get; }
    }
}