namespace FluidFlow.Activities
{
    internal class SpecificationBuildState
    {
        /// <summary>
        /// Gets or sets the mode.
        /// </summary>
        /// <value>
        /// The mode.
        /// </value>
        public SpecificationActivityMode Mode { get; set; }

        /// <summary>
        /// Gets or sets the activity.
        /// </summary>
        /// <value>
        /// The activity.
        /// </value>
        public ISpecificationActivity Activity { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpecificationBuildState"/> class.
        /// </summary>
        /// <param name="mode">The mode.</param>
        /// <param name="activity">The activity.</param>
        public SpecificationBuildState(SpecificationActivityMode mode, ISpecificationActivity activity)
        {
            Mode = mode;
            Activity = activity;
        }
    }
}