using System;
using System.Threading.Tasks;

namespace FluidFlow
{
    internal class BinaryBlobTaskStore : ITaskStateStore
    {
        private readonly ITaskSerializer _serializer;

        public BinaryBlobTaskStore(ITaskSerializer serializer)
        {
            _serializer = serializer;
        }

        /// <summary>
        /// Saves the specified task to the data store.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns></returns>
        public async Task Save(IWorkTask task)
        {
            var blob = (byte[]) _serializer.Serialize(task);
            // TODO: store

            return;
        }

        /// <summary>
        /// Retrieves the serialized task from the data store using the provided ID
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public async Task<T> Get<T>(Guid id) where T : IWorkTask
        {
            // TODO: get blob
            var blob = new byte[0];
            var obj = _serializer.Deserialize<T>(blob);
            return obj;
        }
    }
}