using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluidFlow.Serialization;
using FluidFlow.Specification;

namespace FluidFlow.Activities
{
    [Serializable]
    public class Workflow : Activity, IWorkflowActivity
    {
        private readonly ITaskStateStore _taskStore;
        private readonly WorkflowExecutor _executor;
        private IActivity _lastActivity;

        /// <summary>
        /// Gets the activity queue. 
        /// </summary>
        /// <value>
        /// The activity queue.
        /// </value>
        public Queue<IActivity> ActivityQueue { get; private set; } = new Queue<IActivity>();

        /// <summary>
        /// Initializes an instance of <see cref="Workflow" />
        /// </summary>
        /// <param name="serviceQueue">The state monitor.</param>
        /// <param name="taskStore">The store.</param>
        public Workflow(IServiceQueue serviceQueue, ITaskStateStore taskStore)
        {
            if(serviceQueue == null)
                throw new ArgumentNullException(nameof(serviceQueue));

            if(taskStore == null)
                throw new ArgumentNullException(nameof(taskStore));

            _taskStore = taskStore;
            _executor = new WorkflowExecutor(this, serviceQueue);
        }

        /// <summary>
        /// Executes the task and waits for it to complete.
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public Workflow Do(IActivity task)
        {
            task.Type = ActivityType.SychronizedTask;
            ActivityQueue.Enqueue(task);

            return this;
        }

        /// <summary>
        /// Execute the task and then hand it off to a service queue to monitor state.
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public Workflow WaitFor(IActivity task)
        {
            task.Type = ActivityType.Delayed;
            ActivityQueue.Enqueue(task);

            return this;
        }

        /// <summary>
        /// Execute the task and do not wait for it to finish.
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public Workflow FireAndForget(IActivity task)
        {
            task.Type = ActivityType.FireAndForget;
            ActivityQueue.Enqueue(task);

            return this;
        }

        /// <summary>
        /// Runs the specified task at the same time as the previous task.
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public Workflow And(IActivity task)
        {
            task.Type = ActivityType.Parallel;

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
                parallelCollection.Add(task);

                asList[lastTaskIndex] = parallelCollection;
            }
            else
            {
                asParallelTask.Add(task);
                asList[lastTaskIndex] = asParallelTask;
            }

            ActivityQueue = new Queue<IActivity>(asList);

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
        public Workflow Condition<T>(ISpecification<T> specification, IActivity onSuccess, IActivity onFailure = null)
        {
            var specActivity = new SpecificationActivity<T>(specification, _lastActivity, onSuccess, onFailure);
            ActivityQueue.Enqueue(specActivity);

            return this;
        }

        /// <summary>
        /// Executes the workflow.
        /// </summary>
        /// <returns></returns>
        public override async Task OnRun()
        {
            while (ActivityQueue.Count > 0)
            {
                if (State == ActivityState.Delayed)
                {
                    await SaveState();
                    break;
                }

                _lastActivity = ActivityQueue.Peek();
                await _executor.Execute();
            }
        }

        private async Task SaveState()
        {
            await _taskStore.Save(this);
        }
    }
}
