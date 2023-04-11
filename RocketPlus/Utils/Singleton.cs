namespace RocketPlus.Utils
{
    public class Singleton<T> where T : new()
    {
        private static T? instance;
        private static readonly object locker = new();
        public static T Instace
        {
            get
            {
                if (instance == null)
                {
                    lock (locker)
                    {
                        instance ??= new T();
                    }
                }
                return instance;
            }
        }
    }
}
