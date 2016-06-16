using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluidFlow.Specification;
using Xunit;

namespace FluidFlow.Tests.Specification
{
    public class AndSpecificationTests
    {
        [Fact]
        public void IsSatisfiedBy_TrueTrue_IsTrue()
        {
            // arrange
            var left = SpecificationHelper.GetSpec(true);
            var right = SpecificationHelper.GetSpec(true);
            var spec = new AndSpecification<object>(left, right);

            // act
            var isSatisifed = spec.IsSatisfiedBy(null);

            // assert
            Assert.True(isSatisifed);
        }

        [Fact]
        public void IsSatisfiedBy_TrueFalse_IsFalse()
        {
            // arrange
            var left = SpecificationHelper.GetSpec(true);
            var right = SpecificationHelper.GetSpec(false);
            var spec = new AndSpecification<object>(left, right);

            // act
            var isSatisifed = spec.IsSatisfiedBy(null);

            // assert
            Assert.False(isSatisifed);
        }

        [Fact]
        public void IsSatisfiedBy_FalseTrue_IsFalse()
        {
            // arrange
            var left = SpecificationHelper.GetSpec(false);
            var right = SpecificationHelper.GetSpec(true);
            var spec = new AndSpecification<object>(left, right);

            // act
            var isSatisifed = spec.IsSatisfiedBy(null);

            // assert
            Assert.False(isSatisifed);
        }

        [Fact]
        public void IsSatisfiedBy_FalseFalse_IsFalse()
        {
            // arrange
            var left = SpecificationHelper.GetSpec(false);
            var right = SpecificationHelper.GetSpec(false);
            var spec = new AndSpecification<object>(left, right);

            // act
            var isSatisifed = spec.IsSatisfiedBy(null);

            // assert
            Assert.False(isSatisifed);
        }
    }
}
