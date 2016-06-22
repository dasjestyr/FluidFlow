using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FluidFlow.Activities
{
    public class ParallelActivity : Activity
    {
        private readonly List<IActivity> _tasks;

        /// <summary>
        /// Read-only collection of all tasks to be executed.
        /// </summary>
        public IReadOnlyCollection<IActivity> Tasks => _tasks;

        /// <summary>
        /// Initializes and instance of <see cref="ParallelActivity"/>
        /// </summary>
        public ParallelActivity()
        {
            _tasks = new List<IActivity>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParallelActivity"/> class.
        /// </summary>
        /// <param name="activities">The activities.</param>
        public ParallelActivity(IEnumerable<IActivity> activities)
        {
            var activityList = activities.ToList();
            if (activities == null || !activityList.Any())
                throw new ArgumentNullException(nameof(activities));

            _tasks = activityList;
        }

        /// <summary>
        /// Add a tasks to the collection. Duplicate tasks will be ignored.
        /// </summary>
        /// <param name="task"></param>
        public void Add(IActivity task)
        {
            if(task == null)
                throw new ArgumentNullException(nameof(task));

            if (_tasks.Any(t => t.Id == task.Id))
                return;

            task.Type = ActivityType.Parallel;
            _tasks.Add(task);
        }

        /// <summary>
        /// Starts all tasks and waits until all are completed.
        /// </summary>
        /// <returns></returns>
        protected override async Task OnRun()
        {
            var tasks = _tasks.Select(t => t.Run()).ToList();
            await Task.WhenAll(tasks);
        }
    }
}