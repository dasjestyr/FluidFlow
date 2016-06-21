using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluidFlow.Activities;
using FluidFlow.Serialization;

namespace FluidFlow.Sample1
{
    public class TaskStore : ITaskStateStore
    {
        public async Task Save(IActivity task)
        {
            await Task.Delay(250);
        }

        public Task<T> Get<T>(Guid id) where T : IActivity
        {
            return null;
        }
    }

    public class ServiceQueue : IServiceQueue
    {
        public void AddTask(IDelayedActivity task)
        {
            
        }
    }
}
