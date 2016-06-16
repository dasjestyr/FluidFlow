using System;

namespace FluidFlow.Specification
{
    public class ExpressionSpecification<T> : Specification<T>
    {
        private readonly Func<T, bool> _expression;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionSpecification{T}"/> class.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public ExpressionSpecification(Func<T, bool> expression)
        {
            if(expression == null)
                throw new ArgumentNullException(nameof(expression));

            _expression = expression;
        }

        /// <summary>
        /// Returns whether or not this specification has been satisfied
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public override bool IsSatisfiedBy(T target)
        {
            return _expression(target);
        }
    }
}
