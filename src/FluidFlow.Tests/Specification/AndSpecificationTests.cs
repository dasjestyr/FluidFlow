using System;
using System.Diagnostics.CodeAnalysis;
using FluidFlow.Specification;
using Xunit;

namespace FluidFlow.Tests.Specification
{
    [ExcludeFromCodeCoverage]
    public class AndSpecificationTests
    {
        [Fact]
        public void IsSatisifiedBy_NullTarget_Throws()
        {
            // arrange
            // arrange
            var left = SpecificationHelper.GetSpec(true);
            var right = SpecificationHelper.GetSpec(true);
            var spec = new AndSpecification<object>(left, right);

            // act
            
            // assert
            Assert.Throws<ArgumentNullException>(() => spec.IsSatisfiedBy(null));
        }

        [Fact]
        public void IsSatisfiedBy_TrueTrue_IsTrue()
        {
            // arrange
            var left = SpecificationHelper.GetSpec(true);
            var right = SpecificationHelper.GetSpec(true);
            var spec = new AndSpecification<object>(left, right);

            // act
            var isSatisifed = spec.IsSatisfiedBy(1);

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
            var isSatisifed = spec.IsSatisfiedBy(1);

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
            var isSatisifed = spec.IsSatisfiedBy(1);

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
            var isSatisifed = spec.IsSatisfiedBy(1);

            // assert
            Assert.False(isSatisifed);
        }

        [Fact]
        public void Ctor_LeftNull_Throws()
        {
            // arrange
            var spec = SpecificationHelper.GetSpec(true);

            // act

            // assert
            Assert.Throws<ArgumentNullException>(() => new AndSpecification<object>(null, spec));
        }

        [Fact]
        public void Ctor_RightNull_Throws()
        {
            // arrange
            var spec = SpecificationHelper.GetSpec(true);

            // act

            // assert
            Assert.Throws<ArgumentNullException>(() => new AndSpecification<object>(spec, null));
        }
    }
}
