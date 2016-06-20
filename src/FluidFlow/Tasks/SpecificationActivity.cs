using System;
using System.Threading.Tasks;
using FluidFlow.Ex;
using FluidFlow.Specification;

namespace FluidFlow.Tasks
{
    internal class SpecificationActivity<T> : Activity
    {
        private readonly ISpecification<T> _specification;
        private readonly IActivity _completedActivity;
        private readonly IActivity _runOnSuccess;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpecificationActivity{T}"/> class.
        /// </summary>
        /// <param name="specification">The specification.</param>
        /// <param name="completedActivity">The completed activity.</param>
        /// <param name="runOnSuccess">The run on success.</param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        /// <exception cref="InvalidOperationException">Cannot run specification on an uncompleted activity or an activity with a null result</exception>
        public SpecificationActivity(
            ISpecification<T> specification, 
            IActivity completedActivity, 
            IActivity runOnSuccess)
        {
            if(specification == null)
                throw new ArgumentNullException(nameof(specification));

            if(completedActivity == null)
                throw new ArgumentNullException(nameof(completedActivity));

            if(completedActivity.State != ActivityState.Completed || completedActivity.Result == null)
                throw new InvalidOperationException("Cannot run specification on an uncompleted activity or an activity with a null result");

            _specification = specification;
            _completedActivity = completedActivity;
            _runOnSuccess = runOnSuccess;
        }

        public override async Task OnRun()
        {
            T result;
            if (!_completedActivity.Result.TryCast(out result) || !_specification.IsSatisfiedBy(result))
                return;

            await _runOnSuccess.Run();
        }
    }
}
