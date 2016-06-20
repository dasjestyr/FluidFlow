namespace FluidFlow.Ex
{
    public static class ObjectEx
    {
        public static bool TryCast<T>(this object obj, out T result)
        {
            if (obj is T)
            {
                result = (T) obj;
                return true;
            }

            result = default(T);
            return false;
        }
    }
}
