using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace FluidFlow
{
    internal class DefaultSerializer
    {
        public Task SaveState(Workflow obj)
        {
            var blob = Serialize(obj);
            return null;
        }

        public Task<T> RecoverState<T>(Guid id)
        {
            return null;
        }

        public T Deserizalize<T>(byte[] blob)
        {
            var formatter = new BinaryFormatter();
            using (var s = new MemoryStream(blob))
            {
                var obj = (T) formatter.Deserialize(s);
                return obj;
            }
        }

        private static byte[] Serialize(object obj)
        {
            var formatter = new BinaryFormatter();
            byte[] blob;
            using (var s = new MemoryStream())
            {
                formatter.Serialize(s, obj);
                blob = s.GetBuffer();
            }

            return blob;
        }
    }
}
