using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FluidFlow
{
    public class ParallelWorkTask : WorkTask
    {
        private readonly List<IWorkTask> _tasks;

        /// <summary>
        /// Read-only collection of all tasks to be executed.
        /// </summary>
        public IReadOnlyCollection<IWorkTask> Tasks => _tasks;

        /// <summary>
        /// Initializes and instance of <see cref="ParallelWorkTask"/>
        /// </summary>
        public ParallelWorkTask()
        {
            _tasks = new List<IWorkTask>();
        }

        /// <summary>
        /// Add a tasks to the collection. Duplicate tasks will be ignored.
        /// </summary>
        /// <param name="task"></param>
        public void Add(IWorkTask task)
        {
            if(_tasks.All(t => t.TaskId != task.TaskId))
                _tasks.Add(task);
        }

        /// <summary>
        /// Starts all tasks and waits until all are completed.
        /// </summary>
        /// <returns></returns>
        public override async Task Run()
        {
            var tasks = _tasks.Select(t => t.Run());
            await Task.WhenAll(tasks);
        }
    }
}