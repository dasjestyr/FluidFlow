using FluidFlow.Specification;
using Moq;

namespace FluidFlow.Tests.Specification
{
    public class SpecificationHelper
    {
        public static ISpecification<object> GetSpec(bool returnValue)
        {
            var mock = new Mock<ISpecification<object>>();
            mock.Setup(m => m.IsSatisfiedBy(It.IsAny<object>()))
                .Returns(returnValue);

            return mock.Object;
        }
    }
}