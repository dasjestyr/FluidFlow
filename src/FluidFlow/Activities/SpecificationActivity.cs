﻿using System;
using System.Threading.Tasks;
using FluidFlow.Ex;
using FluidFlow.Specification;

namespace FluidFlow.Activities
{
    [Serializable]
    internal class SpecificationActivity<T> : Activity, ISpecificationActivity
    {
        private readonly ISpecification<T> _specification;
        private readonly IActivity _completedActivity;
        private T _activityResult;
        
        /// <summary>
        /// Gets or sets the success task.
        /// </summary>
        /// <value>
        /// The success task.
        /// </value>
        public IWorkflowActivity SuccessTask { get; internal set; }

        /// <summary>
        /// Gets or sets the fail task.
        /// </summary>
        /// <value>
        /// The fail task.
        /// </value>
        public IWorkflowActivity FailTask { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpecificationActivity{T}" /> class.
        /// </summary>
        /// <param name="specification">The specification.</param>
        /// <param name="completedActivity">The completed activity.</param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Cannot run specification on an uncompleted activity or an activity with a null result
        /// or
        /// </exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException">Cannot run specification on an uncompleted activity or an activity with a null result
        /// or</exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException">Cannot run specification on an uncompleted activity or an activity with a null result
        /// or</exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException">Cannot run specification on an uncompleted activity or an activity with a null result
        /// or</exception>
        public SpecificationActivity(
            ISpecification<T> specification, 
            IActivity completedActivity)
        {
            if(specification == null)
                throw new ArgumentNullException(nameof(specification));

            if(completedActivity == null)
                throw new ArgumentNullException(nameof(completedActivity));
            
            _specification = specification;
            _completedActivity = completedActivity;
        }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns></returns>
        protected override async Task OnRun()
        {
            if (_completedActivity.State != ActivityState.Completed || _completedActivity.Result == null)
                throw new InvalidOperationException("Cannot run specification on an uncompleted activity or an activity with a null result");

            if (!_completedActivity.Result.TryCast(out _activityResult))
                throw new InvalidOperationException($"The result of the provided activity was not of the exepected type (Expected: {typeof(T)}, Actual: {_completedActivity.Result.GetType()}) ");

            State = ActivityState.Completed;
            if (!_specification.IsSatisfiedBy(_activityResult))
            {
                if (FailTask == null)
                    return;

                await FailTask.Run();
                return;
            }

            await SuccessTask.Run();
        }
    }
    
    internal interface ISpecificationActivity : IActivity
    {
        /// <summary>
        /// Gets or sets the success task.
        /// </summary>
        /// <value>
        /// The success task.
        /// </value>
        IWorkflowActivity SuccessTask { get; }

        /// <summary>
        /// Gets or sets the fail task.
        /// </summary>
        /// <value>
        /// The fail task.
        /// </value>
        IWorkflowActivity FailTask { get; }
    }

    internal enum SpecificationActivityMode
    {
        /// <summary>
        /// Designates that we are currently building a success case
        /// </summary>
        SuccessCase,

        /// <summary>
        /// Designates that we are currently building a failure case
        /// </summary>
        FailCase
    }
}
