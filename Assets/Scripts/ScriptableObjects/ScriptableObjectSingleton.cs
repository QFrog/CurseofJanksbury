using System.Linq;
using UnityEngine;

/// <summary>
/// Abstract base class for reload-proof scriptable objects singletons.
/// </summary>
/// <typeparam name="T">Singleton type</typeparam>
public abstract class ScriptableObjectSingleton<T> : ScriptableObject where T : ScriptableObject 
{
    private static T instance = null;
    
    public static T Instance
    {
        get
        {
            if (!instance)
            {
                instance = Resources.FindObjectsOfTypeAll<T>().FirstOrDefault();   
            }
            return instance;
        }
    }
}
