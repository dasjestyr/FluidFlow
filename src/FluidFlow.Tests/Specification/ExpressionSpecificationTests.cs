using System;
using FluidFlow.Specification;
using Xunit;

namespace FluidFlow.Tests.Specification
{
    public class ExpressionSpecificationTests
    {
        [Fact]
        public void IsSatisfiedBy_ReturnsTrue_IsTrue()
        {
            // arrange
            var spec = new ExpressionSpecification<int>(o => o == 10);

            // act
            var isSatisfied = spec.IsSatisfiedBy(10);

            // assert
            Assert.True(isSatisfied);
        }

        [Theory]
        [InlineData(11)]
        [InlineData(101)]
        [InlineData(9)]
        public void IsSatisfiedBy_ReturnsFalse_IsFalse(int testNumber)
        {
            // arrange
            var spec = new ExpressionSpecification<int>(o => o == 10);

            // act
            var isSatisfied = spec.IsSatisfiedBy(testNumber);

            // assert
            Assert.False(isSatisfied);
        }

        [Fact]
        public void Ctor_NullExpression_Throws()
        {
            // arrange

            // act

            // assert
            Assert.Throws<ArgumentNullException>(() => new ExpressionSpecification<int>(null));
        }
    }
}
