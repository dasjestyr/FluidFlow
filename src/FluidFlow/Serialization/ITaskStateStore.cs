﻿using System;
using System.Threading.Tasks;
using FluidFlow.Activities;

namespace FluidFlow.Serialization
{
    public interface ITaskStateStore
    {
        /// <summary>
        /// Saves the specified task to the data store.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns></returns>
        Task Save(IActivity task);

        /// <summary>
        /// Retrieves the serialized task from the data store using the provided ID
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        Task<T> Get<T>(Guid id) where T : IActivity;
    }
}