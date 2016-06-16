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
            var isSatisfied = spec.IsSatisfiedBy(null);

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
            var isSatisfied = spec.IsSatisfiedBy(null);

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
            var isSatisfied = spec.IsSatisfiedBy(null);

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
            var isSatisfied = spec.IsSatisfiedBy(null);

            // assert
            Assert.False(isSatisfied);
        }

        
    }
}
