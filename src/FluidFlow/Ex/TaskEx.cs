using System;
using System.Threading;
using System.Threading.Tasks;

namespace FluidFlow.Ex
{
    public static class TaskEx
    {
        public static async Task<T> WithTimeout<T>(this Task<T> task, TimeSpan timeout)
        {
            var token = new CancellationTokenSource();
            var completedTask = await Task.WhenAny(task, Task.Delay(timeout, token.Token));
            if (completedTask != task)
                throw new TimeoutException("The async task has timed out!");

            token.Cancel();
            return await task;
        }

        public static async Task WithTimeout(this Task task, TimeSpan timeout)
        {
            var token = new CancellationTokenSource();
            var completedTask = await Task.WhenAny(task, Task.Delay(timeout, token.Token));
            if (completedTask != task)
                throw new TimeoutException("The async task has timed out!");

            token.Cancel();
        }
    }
}
