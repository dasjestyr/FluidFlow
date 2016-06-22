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
        private readonly ITaskStateStore _taskStore;
        private readonly IWorkflowExecutor _executor;
        private SpecificationBuildState _conditionalActivity;
        private WorkflowBuilderState _builderState;

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
        public Queue<IActivity> ActivityQueue { get; internal set; } = new Queue<IActivity>();

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

        /// <summary>
        /// Executes the task and waits for it to complete.
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public WorkflowActivity Do(IActivity activity)
        {
            activity.Type = ActivityType.SychronizedTask;
            switch (_builderState)
            {
                case WorkflowBuilderState.BuildingMainWorkflow:
                    ActivityQueue.Enqueue(activity);
                    break;
                case WorkflowBuilderState.BuildingConditionalWorkflow:
                    ChainDo(activity);
                    break;
            }
            
            return this;
        }

        /// <summary>
        /// Execute the task and then hand it off to a service queue to monitor state.
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public WorkflowActivity WaitFor(IActivity activity)
        {
            activity.Type = ActivityType.Delayed;
            switch (_builderState)
            {
                case WorkflowBuilderState.BuildingMainWorkflow:
                    ActivityQueue.Enqueue(activity);
                    break;
                case WorkflowBuilderState.BuildingConditionalWorkflow:
                    ChainWaitFor(activity);
                    break;
            }
            
            return this;
        }

        /// <summary>
        /// Execute the task and do not wait for it to finish.
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public WorkflowActivity FireAndForget(IActivity activity)
        {
            activity.Type = ActivityType.FireAndForget;
            switch (_builderState)
            {
                case WorkflowBuilderState.BuildingMainWorkflow:
                    ActivityQueue.Enqueue(activity);
                    break;
                case WorkflowBuilderState.BuildingConditionalWorkflow:
                    ChainFireAndForget(activity);
                    break;
            }

            return this;
        }

        /// <summary>
        /// Runs the specified task at the same time as the previous task.
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public WorkflowActivity Also(IActivity activity)
        {
            activity.Type = ActivityType.Parallel;
            switch (_builderState)
            {
                case WorkflowBuilderState.BuildingMainWorkflow:
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

                    ActivityQueue = new Queue<IActivity>(asList);
                    break;
                case WorkflowBuilderState.BuildingConditionalWorkflow:
                    ChainAlso(activity);
                    break;
            }

            return this;
        }

        /// <summary>
        /// If the result of the previous activity satisifies the provided specification, run the provided activity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="specification">The specification.</param>
        /// <param name="onSuccess">The activity that will run if the specification is satisfied.</param>
        /// <param name="onFailure">Optional. Runs when the specification is not satisfied.</param>
        /// <returns></returns>
        public WorkflowActivity If<T>(ISpecification<T> specification)
        {
            if (_builderState == WorkflowBuilderState.BuildingConditionalWorkflow)
                throw new InvalidOperationException("You must End() the previous 'IF' condition before beginning a new one");

            _builderState = WorkflowBuilderState.BuildingConditionalWorkflow;

            // fail cases are optional so make sure there is a default action in there
            var defaultFailTask = new ExpressionActivity(ActivityType.FireAndForget, () => Thread.Sleep(1));
            var failWorkflow = new WorkflowActivity(_executor.ServiceQueue, _taskStore);
            failWorkflow.Do(defaultFailTask);

            var specActivity = new SpecificationActivity<T>(specification, ActivityQueue.LastOrDefault())
            {
                SuccessTask = new WorkflowActivity(_executor.ServiceQueue, _taskStore), FailTask = failWorkflow
            };

            _conditionalActivity = new SpecificationBuildState(SpecificationActivityMode.SuccessCase, specActivity);

            return this;
        }

        /// <summary>
        /// Begins a workflow that will run when the specification is not satisfied
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Cannot begin an ELSE workflow without first creating an IF workflow.</exception>
        public WorkflowActivity Else()
        {
            if (_builderState != WorkflowBuilderState.BuildingConditionalWorkflow)
                throw new InvalidOperationException("Cannot begin an ELSE workflow without first creating an IF workflow.");

            _conditionalActivity.Mode = SpecificationActivityMode.FailCase;
            return this;
        }

        /// <summary>
        /// Ends the condition chain
        /// </summary>
        /// <returns></returns>
        public WorkflowActivity EndIf()
        {
            ActivityQueue.Enqueue(_conditionalActivity.Activity);
            _builderState = WorkflowBuilderState.BuildingMainWorkflow;
            _conditionalActivity = null;

            return this;
        }

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

        private static WorkflowActivity TryGetWorkflow(IActivity activity)
        {
            var wf = activity as WorkflowActivity;
            // REFACTOR: This shouldn't ever be an issue since it is controlled internally
            //if (wf == null)
            //    throw new InvalidCastException($"Expected WorkflowActivity, Actual {activity.GetType().Name}");

            return wf;
        }

        private void ChainDo(IActivity activity)
        {
            WorkflowActivity wf;
            if (_conditionalActivity.Mode == SpecificationActivityMode.SuccessCase)
            {
                wf = TryGetWorkflow(_conditionalActivity.Activity.SuccessTask);
                wf = wf.Do(activity);
            }
            else
            {
                wf = TryGetWorkflow(_conditionalActivity.Activity.FailTask);
                wf = wf.Do(activity);
            }

            return;
        }

        private void ChainWaitFor(IActivity activity)
        {
            WorkflowActivity wf;
            if (_conditionalActivity.Mode == SpecificationActivityMode.SuccessCase)
            {
                wf = TryGetWorkflow(_conditionalActivity.Activity.SuccessTask);
                wf = wf.WaitFor(activity);
            }
            else
            {
                wf = TryGetWorkflow(_conditionalActivity.Activity.FailTask);
                wf = wf.WaitFor(activity);
            }

            return;
        }

        private void ChainFireAndForget(IActivity activity)
        {
            WorkflowActivity wf;
            if (_conditionalActivity.Mode == SpecificationActivityMode.SuccessCase)
            {
                wf = TryGetWorkflow(_conditionalActivity.Activity.SuccessTask);
                wf = wf.FireAndForget(activity);
            }
            else
            {
                wf = TryGetWorkflow(_conditionalActivity.Activity.FailTask);
                wf = wf.FireAndForget(activity);
            }

            return;
        }

        private void ChainAlso(IActivity activity)
        {
            WorkflowActivity wf;
            if (_conditionalActivity.Mode == SpecificationActivityMode.SuccessCase)
            {
                wf = TryGetWorkflow(_conditionalActivity.Activity.SuccessTask);
                wf = wf.Also(activity);
            }
            else
            {
                wf = TryGetWorkflow(_conditionalActivity.Activity.FailTask);
                wf = wf.Also(activity);
            }

            return;
        }
    }
}
