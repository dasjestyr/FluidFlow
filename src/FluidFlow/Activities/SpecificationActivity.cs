using System;
using System.Threading.Tasks;
using FluidFlow.Ex;
using FluidFlow.Specification;

namespace FluidFlow.Activities
{
    [Serializable]
    internal class SpecificationActivity<T> : Activity
    {
        private readonly ISpecification<T> _specification;
        private readonly IActivity _onSuccess;
        private readonly IActivity _onFail;
        private readonly T _activityResult;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpecificationActivity{T}" /> class.
        /// </summary>
        /// <param name="specification">The specification.</param>
        /// <param name="completedActivity">The completed activity.</param>
        /// <param name="onSuccess">The run on success.</param>
        /// <param name="onFail">The run on fail.</param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Cannot run specification on an uncompleted activity or an activity with a null result
        /// or
        /// </exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException">Cannot run specification on an uncompleted activity or an activity with a null result</exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException">Cannot run specification on an uncompleted activity or an activity with a null result</exception>
        public SpecificationActivity(
            ISpecification<T> specification, 
            IActivity completedActivity, 
            IActivity onSuccess,
            IActivity onFail)
        {
            if(specification == null)
                throw new ArgumentNullException(nameof(specification));

            if(completedActivity == null)
                throw new ArgumentNullException(nameof(completedActivity));

            if(onSuccess == null)
                throw new ArgumentNullException(nameof(onSuccess));

            if(completedActivity.State != ActivityState.Completed || completedActivity.Result == null)
                throw new InvalidOperationException("Cannot run specification on an uncompleted activity or an activity with a null result");

            if(!completedActivity.Result.TryCast(out _activityResult))
                throw new InvalidOperationException($"The result of the provided activity was not of the exepected type (Expected: {typeof(T)}, Actual: {completedActivity.Result.GetType()}) ");

            _specification = specification;
            _onSuccess = onSuccess;
            _onFail = onFail;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpecificationActivity{T}"/> class.
        /// </summary>
        /// <param name="specification">The specification.</param>
        /// <param name="completedActivity">The completed activity.</param>
        /// <param name="onSuccess">The run on success.</param>
        public SpecificationActivity(
            ISpecification<T> specification,
            IActivity completedActivity,
            IActivity onSuccess) 
                : this(specification, completedActivity, onSuccess, null)
        {
        }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns></returns>
        public override async Task OnRun()
        {
            State = ActivityState.Completed;
            if (!_specification.IsSatisfiedBy(_activityResult))
            {
                if (_onFail == null)
                    return;

                await _onFail.Run();
                return;
            }

            await _onSuccess.Run();
        }
    }
}
