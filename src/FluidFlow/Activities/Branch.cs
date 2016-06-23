namespace FluidFlow.Activities
{
    internal class Branch
    {
        /// <summary>
        /// Gets or sets the previous branch.
        /// </summary>
        /// <value>
        /// The previous.
        /// </value>
        public Branch Previous { get; set; }

        /// <summary>
        /// Gets or sets the mode.
        /// </summary>
        /// <value>
        /// The mode.
        /// </value>
        public SpecificationActivityMode Mode { get; set; }

        /// <summary>
        /// Gets or sets the activity that should be executed.
        /// </summary>
        /// <value>
        /// The activity.
        /// </value>
        public ISpecificationActivity Activity { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Branch" /> class.
        /// </summary>
        /// <param name="previous">The previous.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="activity">The activity.</param>
        public Branch(
            Branch previous,
            SpecificationActivityMode mode, 
            ISpecificationActivity activity)
        {
            Previous = previous;
            Mode = mode;
            Activity = activity;
        }
    }
}