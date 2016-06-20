namespace FluidFlow.Activities
{
    public interface IDelayedActivity : IActivity
    {
        /// <summary>
        /// The state monitor.
        /// </summary>
        IStateMonitor StateMonitor { get; }
    }
}