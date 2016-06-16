using System;
using System.Diagnostics.CodeAnalysis;
using FluidFlow.Specification;
using Xunit;

namespace FluidFlow.Tests.Specification
{
    [ExcludeFromCodeCoverage]
    public class NotSpecificationTests
    {
        [Fact]
        public void IsSatisfiedBy_False_IsTrue()
        {
            // arrange
            var left = SpecificationHelper.GetSpec(false);
            var spec = new NotSpecification<object>(left);

            // act
            var isSatisifed = spec.IsSatisfiedBy(1);

            // assert
            Assert.True(isSatisifed);
        }

        [Fact]
        public void IsSatisfiedBy_True_IsFalse()
        {
            // arrange
            var left = SpecificationHelper.GetSpec(true);
            var spec = new NotSpecification<object>(left);

            // act
            var isSatisifed = spec.IsSatisfiedBy(1);

            // assert
            Assert.False(isSatisifed);
        }

        [Fact]
        public void IsSatisfiedBy_NullTarget_Throws()
        {
            // arrange
            var left = SpecificationHelper.GetSpec(true);
            var spec = new NotSpecification<object>(left);

            // act

            // assert
            Assert.Throws<ArgumentNullException>(() => spec.IsSatisfiedBy(null));
        }

        [Fact]
        public void Ctor_NullSpec_Throws()
        {
            // arrange

            // act

            // assert
            Assert.Throws<ArgumentNullException>(() => new NotSpecification<object>(null));
        }
    }
}
