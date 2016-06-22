namespace FluidFlow.Activities
{
    internal class SpecificationBranch
    {
        public SpecificationBranch Previous { get; set; }

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
        /// Initializes a new instance of the <see cref="SpecificationBranch" /> class.
        /// </summary>
        /// <param name="previous">The previous.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="activity">The activity.</param>
        public SpecificationBranch(
            SpecificationBranch previous,
            SpecificationActivityMode mode, 
            ISpecificationActivity activity)
        {
            Previous = previous;
            Mode = mode;
            Activity = activity;
        }
    }
}