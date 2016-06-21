using FluidFlow.Sample1.Activities;
using FluidFlow.Specification;

namespace FluidFlow.Sample1.Specifications
{
    public class ManagerApprovedRequestSpec : Specification<StepApproval>
    {
        public override bool IsSatisfiedBy(StepApproval target)
        {
            return target.IsApproved;
        }
    }
}
