using System.Threading.Tasks;
using FluidFlow.Activities;
using FluidFlow.Sample1.Messaging;

namespace FluidFlow.Sample1.Activities
{
    public class CreateChangeRequestActivity : Activity
    {
        protected override async Task OnRun()
        {
            await Task.Delay(1000);
            var e = new ActivityRunEvent
            {
                EventName = "CreateChangeRequestActivity.Run()",
                Message = "The change request was created"
            };

            MessageBroker.Broadcast(e);
        }
    }

    public class RequestManagerApprovalActivity : Activity, IDelayedActivity
    {
        public IStateMonitor StateMonitor { get; }

        public RequestManagerApprovalActivity(IStateMonitor stateMonitor)
        {
            StateMonitor = stateMonitor;
        }

        protected override async Task OnRun()
        {
            await Task.Delay(1000);
            Result = new StepApproval(true);
            var e = new ActivityRunEvent
            {
                EventName = "RequestManagerApprovalActivity",
                Message = "A request for manager approval was sent."
            };

            MessageBroker.Broadcast(e);
        }
    }

    public class NotifyRequestActivity : Activity
    {
        private readonly string _name;

        public NotifyRequestActivity(string name)
        {
            _name = name;
        }

        protected override async Task OnRun()
        {
            await Task.Delay(100);
            var e = new ActivityRunEvent
            {
                EventName = "RequestTeamApproval",
                Message = $"{_name} was notified of the change request!"
            };

            MessageBroker.Broadcast(e);
        }
    }

    public class RequestQualityReviewActivity : Activity, IDelayedActivity
    {
        public IStateMonitor StateMonitor { get; }

        public RequestQualityReviewActivity(IStateMonitor stateMonitor)
        {
            StateMonitor = stateMonitor;
        }

        protected override async Task OnRun()
        {
            await Task.Delay(500);
            var e = new ActivityRunEvent
            {
                EventName = "RequestQualityReview.Run()",
                Message = "A request was sent to QA for review"
            };

            MessageBroker.Broadcast(e);
        }
    }

    public class RequestBoardApprovalActivity : Activity, IDelayedActivity
    {
        public IStateMonitor StateMonitor { get; }

        public RequestBoardApprovalActivity(IStateMonitor stateMonitor)
        {
            StateMonitor = stateMonitor;
        }

        protected override async Task OnRun()
        {
            await Task.Delay(500);
            Result = new StepApproval(true);
            var e = new ActivityRunEvent
            {
                EventName = "RequestBoardApproval",
                Message = "A request was sent to the board for approval"
            };

            MessageBroker.Broadcast(e);
        }
    }

    public class PublishChangesActivity : Activity
    {
        protected override async Task OnRun()
        {
            await Task.Delay(500);
            var e = new ActivityRunEvent
            {
                EventName = "PublishChangesActivity",
                Message = "Changes were published"
            };

            MessageBroker.Broadcast(e);
        }
    }
}
