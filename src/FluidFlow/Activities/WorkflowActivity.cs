using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using FluidFlow.Serialization;
using FluidFlow.Specification;

namespace FluidFlow.Activities
{
    [Serializable]
    public class WorkflowActivity : Activity, IWorkflowActivity
    {
        private readonly WorkflowActivity _parentFlow;
        private readonly ITaskStateStore _taskStore;
        private readonly IWorkflowExecutor _executor;
        private SpecificationBranch _specificationBranch;
        
        /// <summary>
        /// Gets or sets the last activity.
        /// </summary>
        /// <value>
        /// The last activity.
        /// </value>
        public IActivity LastActivity { get; set; }

        /// <summary>
        /// Gets the activity queue. 
        /// </summary>
        /// <value>
        /// The activity queue.
        /// </value>
        public Queue<IActivity> ActivityQueue { get; set; } = new Queue<IActivity>();

        #region -- Ctors --

        /// <summary>
        /// Initializes an instance of <see cref="WorkflowActivity" />
        /// </summary>
        /// <param name="serviceQueue">The state monitor.</param>
        /// <param name="taskStore">The store.</param>
        public WorkflowActivity(IServiceQueue serviceQueue, ITaskStateStore taskStore)
        {
            if(serviceQueue == null)
                throw new ArgumentNullException(nameof(serviceQueue));

            if(taskStore == null)
                throw new ArgumentNullException(nameof(taskStore));

            _taskStore = taskStore;
            _executor = new WorkflowExecutor(this, serviceQueue);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowActivity"/> class.
        /// </summary>
        /// <param name="serviceQueue">The service queue.</param>
        /// <param name="taskStore">The task store.</param>
        /// <param name="parentFlow">The parent flow.</param>
        public WorkflowActivity(IServiceQueue serviceQueue, ITaskStateStore taskStore, WorkflowActivity parentFlow)
            : this(serviceQueue, taskStore)
        {
            _parentFlow = parentFlow;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowActivity"/> class. Used for testing.
        /// </summary>
        /// <param name="serviceQueue">The service queue.</param>
        /// <param name="taskStore">The task store.</param>
        /// <param name="executor">The executor.</param>
        /// <exception cref="ArgumentNullException"></exception>
        internal WorkflowActivity(IServiceQueue serviceQueue, ITaskStateStore taskStore, IWorkflowExecutor executor)
            : this(serviceQueue, taskStore)
        {
            if(executor == null)
                throw new ArgumentNullException(nameof(executor));

            _executor = executor;
        }

        #endregion

        #region -- Builder --

        /// <summary>
        /// Executes the task and waits for it to complete.
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public IWorkflowActivity Do(IActivity activity)
        {
            activity.Type = ActivityType.SychronizedTask;
            TargetWorkflow.ActivityQueue.Enqueue(activity);
            return TargetWorkflow;
        }

        /// <summary>
        /// Execute the task and then hand it off to a service queue to monitor state.
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public IWorkflowActivity WaitFor(IActivity activity)
        {
            activity.Type = ActivityType.Delayed;
            TargetWorkflow.ActivityQueue.Enqueue(activity);
            
            return TargetWorkflow;
        }

        /// <summary>
        /// Execute the task and do not wait for it to finish.
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public IWorkflowActivity FireAndForget(IActivity activity)
        {
            activity.Type = ActivityType.FireAndForget;
            TargetWorkflow.ActivityQueue.Enqueue(activity);
            return TargetWorkflow;
        }

        /// <summary>
        /// Runs the specified task at the same time as the previous task.
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public IWorkflowActivity Also(IActivity activity)
        {
            activity.Type = ActivityType.Parallel;
            var asList = ActivityQueue.ToList();

            var lastTask = asList.LastOrDefault();
            if (lastTask == null || lastTask.Type == ActivityType.Delayed)
                throw new InvalidOperationException("Cannot add a parallel task to a delayed task or an empty workflow.");

            var lastTaskIndex = asList.IndexOf(lastTask);
            var asParallelTask = lastTask as ParallelActivity;

            // make sure the last task is a parallel task and add this task to it
            if (asParallelTask == null)
            {
                var parallelCollection = new ParallelActivity();
                parallelCollection.Add(lastTask);
                parallelCollection.Add(activity);

                asList[lastTaskIndex] = parallelCollection;
            }
            else
            {
                asParallelTask.Add(activity);
                asList[lastTaskIndex] = asParallelTask;
            }

            TargetWorkflow.ActivityQueue = new Queue<IActivity>(asList);
            return TargetWorkflow;
        }

        #endregion

        #region -- Workflow Branching --

        /*************************************** IMPORTANT DESIGN NOTES *****************************************
         * It is important to remember that when working with branches, the workflow is actually being built
         * on either the success or fail branches of the PARENT. There is no scenario where IF/ELSE/ENDIF should
         * be operating on "this"
         * **************************************************************************************************** */

        /// <summary>
        /// If the result of the previous activity satisifies the provided specification, run the provided activity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="specification">The specification.</param>
        /// <param name="onSuccess">The activity that will run if the specification is satisfied.</param>
        /// <param name="onFailure">Optional. Runs when the specification is not satisfied.</param>
        /// <returns></returns>
        public IWorkflowActivity If<T>(ISpecification<T> specification)
        {
            // REFACTOR: save current branch state (https://github.com/dasjestyr/FluidFlow/issues/1)
            // specification activity is the fork with success and fail workflows

            // fail cases are optional so make sure there is a default action in there
            var defaultFailTask = new ExpressionActivity(ActivityType.FireAndForget, () => Thread.Sleep(1));
            var failWorkflow = new WorkflowActivity(_executor.ServiceQueue, _taskStore, this);
            failWorkflow.Do(defaultFailTask);

            var specActivity = new SpecificationActivity<T>(specification, ActivityQueue.LastOrDefault())
            {
                SuccessTask = new WorkflowActivity(_executor.ServiceQueue, _taskStore, this),
                FailTask = failWorkflow
            };

            // create the fork
            _specificationBranch =
                new SpecificationBranch(
                _specificationBranch,
                SpecificationActivityMode.SuccessCase, // we start with the success case
                specActivity);

            return TargetWorkflow;
        }

        /// <summary>
        /// Begins a workflow that will run when the specification is not satisfied
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Cannot begin an ELSE workflow without first creating an IF workflow.</exception>
        public IWorkflowActivity Else()
        {
            // IMPORTANT DESIGN NOTE: else always belongs to the parent IF branch
            if(_parentFlow?._specificationBranch == null)
                throw new InvalidOperationException("Cannot create and ELSE branch without a preceeding IF branch");

            _parentFlow._specificationBranch.Mode = SpecificationActivityMode.FailCase;
            return _parentFlow;
        }

        /// <summary>
        /// Ends the condition chain
        /// </summary>
        /// <returns></returns>
        public IWorkflowActivity EndIf()
        {
            // NOTE: EndIf always belongs to the parent IF branch
            _parentFlow.ActivityQueue.Enqueue(_parentFlow._specificationBranch.Activity);
            _parentFlow._specificationBranch = _parentFlow._specificationBranch.Previous;

            return _parentFlow;
        }

        #endregion

        // determines the current workflow builder that is being worked
        private IWorkflowActivity TargetWorkflow => _specificationBranch == null
            ? this
            : _specificationBranch.Mode == SpecificationActivityMode.SuccessCase
                ? _specificationBranch.Activity.SuccessTask
                : _specificationBranch.Activity.FailTask;
        
        /// <summary>
        /// Executes the workflow.
        /// </summary>
        /// <returns></returns>
        protected override async Task OnRun()
        {
            if (!ActivityQueue.Any())
                throw new InvalidOperationException("Nothing in the queue!");

            while (ActivityQueue.Count > 0)
            {
                if (State == ActivityState.Delayed)
                    break;

                LastActivity = ActivityQueue.Peek();
                await _executor.Execute();
            }
        }

        /// <summary>
        /// Saves the state.
        /// </summary>
        /// <returns></returns>
        public async Task SaveState()
        {
            await _taskStore.Save(this);
        }
    }
}
