namespace FluidFlow.Specification
{
    public abstract class Specification<T> : ISpecification<T>
    {
        /// <summary>
        /// Returns whether or not this specification has been satisfied
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public abstract bool IsSatisfiedBy(T target);

        /// <summary>
        /// Specifies that this instance AND the specified specification must be satisfied
        /// </summary>
        /// <param name="specification">The specification.</param>
        /// <returns></returns>
        public ISpecification<T> And(ISpecification<T> specification)
        {
            return new AndSpecification<T>(this, specification);
        }

        /// <summary>
        /// Specifies that this instance OR the specified specification must be satisfied
        /// </summary>
        /// <param name="specification">The specification.</param>
        /// <returns></returns>
        public ISpecification<T> Or(ISpecification<T> specification)
        {
            return new OrSpecification<T>(this, specification);
        }

        /// <summary>
        /// Specifies that the provided specification must be NOT satisfied
        /// </summary>
        /// <param name="specification">The specification.</param>
        /// <returns></returns>
        public ISpecification<T> Not(ISpecification<T> specification)
        {
            return new NotSpecification<T>(specification);
        }
    }
}