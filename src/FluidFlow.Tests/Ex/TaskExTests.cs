using System;
using System.Threading;
using System.Threading.Tasks;
using FluidFlow.Ex;
using Xunit;

namespace FluidFlow.Tests.Ex
{
    public class TaskExTests
    {
        [Fact]
        public async void WithTimeout_ActionTNoTimeout_DoesNotThrow()
        {
            // arrange
            var mainTask = GetTask<int>(TimeSpan.FromMilliseconds(1));

            // act

            // assert
            await mainTask.WithTimeout(TimeSpan.FromSeconds(2));
        }

        [Fact]
        public async void WithTimeout_ActionOfTTimeout_Throws()
        {
            // arrange
            var mainTask = GetTask<int>(TimeSpan.FromSeconds(2));
            
            // act
            
            // assert
            await Assert.ThrowsAsync<TimeoutException>(() => mainTask.WithTimeout(TimeSpan.FromSeconds(1)));
        }

        [Fact]
        public async void WithTimeout_ActionTimeout_Throws()
        {
            // arrange
            var mainTask= new Task(() => Thread.Sleep(2000));

            // act

            // assert
            await Assert.ThrowsAsync<TimeoutException>(() => mainTask.WithTimeout(TimeSpan.FromSeconds(1)));
        }

        [Fact]
        public async void WithTimeout_ActionNoTimeout_DoesNotThrow()
        {
            // arrange
            var mainTask = GetTask(TimeSpan.FromMilliseconds(1));

            // act

            // assert
            await mainTask.WithTimeout(TimeSpan.FromSeconds(2));
        }

        private static async Task GetTask(TimeSpan delay)
        {
            await Task.Delay(delay);
        }

        private static async Task<T> GetTask<T>(TimeSpan delay)
        {
            await Task.Delay(delay);
            return default(T);
        }
    }
}
