using UnityEngine;

public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<T>();

                if (instance == null)
                {
                    GameObject singletonObject = new GameObject(typeof(T).Name);
                    instance = singletonObject.AddComponent<T>();
                    DontDestroyOnLoad(singletonObject);
                }
            }
            return instance;
        }
    }

    /// <summary>
    /// <remake>
    ///   If you are overriding this method, include the following snippet:
    ///   base.Awake();
    ///   if( Instance != this )
    ///       return;
    /// </remake>
    /// </summary>
    protected virtual void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            return;
        }
        instance = this as T;
        DontDestroyOnLoad(this);
    }

    protected virtual void OnDestroy()
    {
        if (ReferenceEquals(instance, this))
            instance = null;
    }
}