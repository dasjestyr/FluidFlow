﻿using System.Threading.Tasks;

namespace FluidFlow.Specification
{
    public interface ISpecification<T>
    {
        /// <summary>
        /// Returns whether or not this specification has been satisfied
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        bool IsSatisfiedBy(T target);

        /// <summary>
        /// Returns whether or not this specification has been satisfied, asynchronously
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        Task<bool> IsSatisfiedByAsync(T target);

            /// <summary>
        /// Specifies that this instance AND the specified specification must be satisfied
        /// </summary>
        /// <param name="specification">The specification.</param>
        /// <returns></returns>
        ISpecification<T> And(ISpecification<T> specification);

        /// <summary>
        /// Specifies that this instance OR the specified specification must be satisfied
        /// </summary>
        /// <param name="specification">The specification.</param>
        /// <returns></returns>
        ISpecification<T> Or(ISpecification<T> specification);

        /// <summary>
        /// Specifies that the provided specification must be NOT satisfied
        /// </summary>
        /// <param name="specification">The specification.</param>
        /// <returns></returns>
        ISpecification<T> Not(ISpecification<T> specification);
    }
}
