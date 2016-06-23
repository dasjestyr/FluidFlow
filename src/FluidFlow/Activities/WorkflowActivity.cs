using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluidFlow.Serialization;
using FluidFlow.Specification;

namespace FluidFlow.Activities
{
    [Serializable]
    public class WorkflowActivity : Activity, IWorkflowActivity
    {
        private readonly ITaskStateStore _taskStore;
        private readonly IWorkflowExecutor _executor;

        // used for branching
        private readonly WorkflowActivity _parentFlow;
        private Branch _branch;
        
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
        /// <returns></returns>
        public IWorkflowActivity If<T>(ISpecification<T> specification)
        {
            var specActivity = new SpecificationActivity<T>(specification, ActivityQueue.LastOrDefault())
            {
                SuccessTask = GetChild(),
                FailTask = GetChild()
            };

            // create the fork
            _branch = new Branch(
                _branch,
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
            if(_parentFlow?._branch == null)
                throw new InvalidOperationException("Cannot create and ELSE branch without a preceeding IF branch");

            _parentFlow._branch.Mode = SpecificationActivityMode.FailCase;

            // we return the parent since it is the builder being used by the client. The mode will tell it which WF to build upon
            return _parentFlow;
        }

        /// <summary>
        /// Ends the condition chain
        /// </summary>
        /// <returns></returns>
        public IWorkflowActivity EndIf()
        {
            // we actually queue the SpecificationActivity since it it will control which workflow is executed
            // based on the outcome of IsSatisified()
            _parentFlow.ActivityQueue.Enqueue(_parentFlow._branch.Activity);
            _parentFlow._branch = _parentFlow._branch.Previous;

            // we return the parent since it is the builder being used by the client. The mode will tell it which WF to build upon
            return _parentFlow;
        }

        #endregion

        // determines the current workflow builder that is being worked
        private IWorkflowActivity TargetWorkflow => _branch == null
            ? this
            : _branch.Mode == SpecificationActivityMode.SuccessCase
                ? _branch.Activity.SuccessTask
                : _branch.Activity.FailTask;
        
        /// <summary>
        /// Executes the workflow.
        /// </summary>
        /// <returns></returns>
        protected override async Task OnRun()
        {
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

        /// <summary>
        /// Spawns a child workflow that utilize the same resources.
        /// </summary>
        /// <returns></returns>
        public IWorkflowActivity GetChild()
        {
            return new WorkflowActivity(_executor.ServiceQueue, _taskStore, this);
        }
    }
}
