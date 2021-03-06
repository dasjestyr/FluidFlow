﻿using System;

namespace FluidFlow.Specification
{
    public class OrSpecification<T> : Specification<T>
    {
        private readonly ISpecification<T> _left;
        private readonly ISpecification<T> _right;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrSpecification{T}"/> class.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        public OrSpecification(ISpecification<T> left, ISpecification<T> right)
        {
            if(left == null)
                throw new ArgumentNullException(nameof(left));

            if(right == null)
                throw new ArgumentNullException(nameof(right));

            _left = left;
            _right = right;
        }

        /// <summary>
        /// Returns whether or not this specification has been satisfied
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public override bool IsSatisfiedBy(T target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            return _left.IsSatisfiedBy(target) || _right.IsSatisfiedBy(target);
        }
    }
}