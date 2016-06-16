using System;

namespace FluidFlow.Specification
{
    public class NotSpecification<T> : Specification<T>
    {
        private readonly ISpecification<T> _spec;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotSpecification{T}"/> class.
        /// </summary>
        /// <param name="spec">The spec.</param>
        public NotSpecification(ISpecification<T> spec)
        {
            if (spec == null)
                throw new ArgumentNullException(nameof(spec));

            _spec = spec;
        }

        /// <summary>
        /// Returns whether or not this specification has been satisfied
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public override bool IsSatisfiedBy(T target)
        {
            if(target == null)
                throw new ArgumentNullException(nameof(target));

            return !_spec.IsSatisfiedBy(target);
        }
    }
}