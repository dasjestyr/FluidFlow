using System;
using System.Diagnostics.CodeAnalysis;
using FluidFlow.Ex;
using FluidFlow.Specification;
using Xunit;

namespace FluidFlow.Tests.Specification
{
    [ExcludeFromCodeCoverage]
    public class ExpressionSpecificationTests
    {
        [Fact]
        public async void IsSatisfiedByAsync_DoesNotDeadlock()
        {
            // arrange
            var spec = new ExpressionSpecification<int>(o => o == 10);

            // act
            var result = await spec
                .IsSatisfiedByAsync(10)
                .WithTimeout(TimeSpan.FromSeconds(5000));

            // assert
            Assert.True(result);
        }

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

        [Fact]
        public void IsSatisfiedBy_NullTarget_Throws()
        {
            // arrange
            var spec = new ExpressionSpecification<object>(o => true);

            // act

            // assert
            Assert.Throws<ArgumentNullException>(() => spec.IsSatisfiedBy(null));
        }
    }
}
