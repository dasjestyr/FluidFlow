using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace FluidFlow
{
    public class BinarySerializer : ITaskSerializer
    {
        public object Serialize(IWorkTask obj)
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

        public T Deserialize<T>(byte[] blob)
            where T : IWorkTask
        {
            var formatter = new BinaryFormatter();
            using (var s = new MemoryStream(blob))
            {
                var obj = (T)formatter.Deserialize(s);
                return obj;
            }
        }
    }
}
