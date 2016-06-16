using System;
using FluidFlow.Specification;
using Xunit;

namespace FluidFlow.Tests.Specification
{
    public class OrSpecificationTests
    {
        [Fact]
        public void IsSatisfiedBy_TrueTrue_ReturnsTrue()
        {
            // arrange
            var left = SpecificationHelper.GetSpec(true);
            var right = SpecificationHelper.GetSpec(true);
            var spec = new OrSpecification<object>(left, right);

            // act
            var isSatisfied = spec.IsSatisfiedBy(1);

            // assert
            Assert.True(isSatisfied);
        }

        [Fact]
        public void IsSatisfiedBy_TrueFalse_ReturnsTrue()
        {
            // arrange
            var left = SpecificationHelper.GetSpec(true);
            var right = SpecificationHelper.GetSpec(false);
            var spec = new OrSpecification<object>(left, right);

            // act
            var isSatisfied = spec.IsSatisfiedBy(1);

            // assert
            Assert.True(isSatisfied);
        }

        [Fact]
        public void IsSatisfiedBy_FalseTrue_ReturnsTrue()
        {
            // arrange
            var left = SpecificationHelper.GetSpec(false);
            var right = SpecificationHelper.GetSpec(true);
            var spec = new OrSpecification<object>(left, right);

            // act
            var isSatisfied = spec.IsSatisfiedBy(1);

            // assert
            Assert.True(isSatisfied);
        }

        [Fact]
        public void IsSatisfiedBy_FalseFalse_ReturnsFalse()
        {
            // arrange
            var left = SpecificationHelper.GetSpec(false);
            var right = SpecificationHelper.GetSpec(false);
            var spec = new OrSpecification<object>(left, right);

            // act
            var isSatisfied = spec.IsSatisfiedBy(1);

            // assert
            Assert.False(isSatisfied);
        }

        [Fact]
        public void IsSatisfiedBy_NullTarget_Throws()
        {
            // arrange
            var left = SpecificationHelper.GetSpec(true);
            var right = SpecificationHelper.GetSpec(true);
            var spec = new OrSpecification<object>(left, right);

            // act

            // assert
            Assert.Throws<ArgumentNullException>(() => spec.IsSatisfiedBy(null));
        }

        [Fact]
        public void Ctor_LeftNull_Throws()
        {
            // arrange
            var spec = SpecificationHelper.GetSpec(true);

            // act

            // assert
            Assert.Throws<ArgumentNullException>(() => new OrSpecification<object>(null, spec));
        }

        [Fact]
        public void Ctor_RightNull_Throws()
        {
            // arrange
            var spec = SpecificationHelper.GetSpec(true);

            // act

            // assert
            Assert.Throws<ArgumentNullException>(() => new OrSpecification<object>(spec, null));
        }
    }
}
