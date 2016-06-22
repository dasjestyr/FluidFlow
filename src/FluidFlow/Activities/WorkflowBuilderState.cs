namespace FluidFlow.Activities
{
    internal enum WorkflowBuilderState
    {
        /// <summary>
        /// Currently building the main branch
        /// </summary>
        BuildingMainWorkflow,

        /// <summary>
        /// Currently building a branch that will run when a condition is or is not met.
        /// </summary>
        BuildingConditionalWorkflow
    }
}