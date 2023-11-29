public abstract class Singleton<T> where T : new()
{
    private static T instance;
    private static readonly object padlock = new object();

    public static T Instance
    {
        get
        {
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new T();
                }
                return instance;
            }
        }
    }
}