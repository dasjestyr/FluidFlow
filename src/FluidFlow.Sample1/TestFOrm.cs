using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluidFlow.Activities;
using FluidFlow.Sample1.Activities;
using FluidFlow.Sample1.Messaging;
using FluidFlow.Sample1.Specifications;

namespace FluidFlow.Sample1
{
    public partial class TestForm : Form
    {
        public TestForm()
        {
            InitializeComponent();
            MessageBroker.Subscribe(typeof(ActivityRunEvent), OutputMessage);
        }

        private async Task RunWorkFlow()
        {
            var serviceQueue = new ServiceQueue();
            var taskStore = new TaskStore();

            var createChangeRequest = new CreateChangeRequestActivity();
            var requestManagerApproval = new RequestManagerApprovalActivity(null);
            
            var notifyTasks = GetDepartmentEmailAddresses().Select(e => new NotifyRequestActivity(e));

            var notifyDepartment = new ParallelActivity(notifyTasks);
            var requestQualityReview = new RequestQualityReviewActivity(null);

            var requestBoardApproval = new RequestBoardApprovalActivity(null);
            var publishChanges = new PublishChangesActivity();

            // specs
            var managerApprovedRequest = new ManagerApprovedRequestSpec();
            var qualityReviewApproved = new QualityReviewApprovedSpec();
            var boardApprovedChanges = new BoardApprovedChangesSpec();

            // define the workflow
            var workflow = new WorkflowActivity(serviceQueue, taskStore)
                .Do(createChangeRequest)
                .FireAndForget(notifyDepartment)
                .WaitFor(requestManagerApproval)
                    .If(managerApprovedRequest)
                        .WaitFor(requestQualityReview)
                        .If(qualityReviewApproved)
                            .WaitFor(requestBoardApproval)
                            .If(boardApprovedChanges)
                                .Do(publishChanges)
                                .Also(notifyDepartment)
                            .EndIf()
                        .EndIf()
                    .Else()
                        .Do(notifyDepartment)
                    .EndIf();

            await workflow.Run();
        }

        private static IEnumerable<string> GetDepartmentEmailAddresses()
        {
            return new List<string>
            {
                "jon@castleblack.com", "petyr@eeyrie.com", "arya@freecities.com"
            };
        }

        private void btnCreateChangeRequest_Click(object sender, EventArgs e)
        {
            Task.Run(RunWorkFlow);
            btnCreateChangeRequest.Enabled = false;
        }

        private void OutputMessage(BrokerEvent ev)
        {
            var args = ev as ActivityRunEvent;
            if(args == null) return;

            Invoke((Action)(() => txtOutput.AppendText(ev.Message + Environment.NewLine)));    
        }
    }
}
