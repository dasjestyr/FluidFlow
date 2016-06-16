using FluidFlow.Specification;
using Xunit;

namespace FluidFlow.Tests.Specification
{
    public class NotSpecificationTests
    {
        [Fact]
        public void IsSatisfiedBy_False_IsTrue()
        {
            // arrange
            var left = SpecificationHelper.GetSpec(false);
            var spec = new NotSpecification<object>(left);

            // act
            var isSatisifed = spec.IsSatisfiedBy(null);

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
            var isSatisifed = spec.IsSatisfiedBy(null);

            // assert
            Assert.False(isSatisifed);
        }
    }
}
