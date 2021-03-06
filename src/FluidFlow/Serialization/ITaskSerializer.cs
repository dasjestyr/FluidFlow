﻿using FluidFlow.Activities;

namespace FluidFlow.Serialization
{
    public interface ITaskSerializer
    {
        /// <summary>
        /// Serializes the specified task.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns></returns>
        object Serialize(IActivity task);

        /// <summary>
        /// Deserializes the specified to the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="blob">The BLOB.</param>
        /// <returns></returns>
        T Deserialize<T>(byte[] blob) where T : IActivity;
    }
}