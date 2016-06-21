using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluidFlow.Activities;
using FluidFlow.Sample1.Activities;
using FluidFlow.Sample1.Specifications;

namespace FluidFlow.Sample1
{
    public partial class TestForm : Form
    {
        public TestForm()
        {
            InitializeComponent();
        }

        private async Task RunWorkFlow()
        {
            var serviceQueue = new ServiceQueue();
            var taskStore = new TaskStore();

            var createChangeRequest = new CreateChangeRequestActivity();
            var requestManagerApproval = new RequestManagerApprovalActivity(null);
            var notifyTasks = GetDepartmentEmailAddresses().Select(e => new NotifyRequestActivity(e));

            var notifyDepartment = new ParallelActivity(notifyTasks);
            var requestQualityReview = new RequestQualityReviewActivity(null) { Type = ActivityType.Delayed };

            var requestBoardApproval = new RequestBoardApprovalActivity(null);

            var workflow = new WorkflowActivity(serviceQueue, taskStore)
                .Do(createChangeRequest)
                .FireAndForget(notifyDepartment)
                .WaitFor(requestManagerApproval)
                    .If(new ManagerApprovedRequestSpec())
                        .WaitFor(requestQualityReview)
                    .Else()
                        .Do(notifyDepartment)
                    .EndIf()
                .WaitFor(requestBoardApproval);

            await workflow.Run();
        }

        private static IEnumerable<string> GetDepartmentEmailAddresses()
        {
            return new List<string>
            {
                "jon@castleblack.com", "petyr@eeyrie.com", "arya@freecities.com"
            };
        }
    }
}
