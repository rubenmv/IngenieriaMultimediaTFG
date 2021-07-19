using UnityEngine;
using System.Collections;

public class Singleton<T> : MonoBehaviour where T : Component
{
  [SerializeField]
  bool
    _persistent = true;

  private static T _instance;
  public static bool IsInstantianted { get { return _instance != null; } }

  public static T Instance
  {
    get
    {
      if(_instance == null)
      {
        _instance = (T)FindObjectOfType(typeof(T));
        if(_instance == null)
        {
          T prefab = Resources.Load(typeof(T).Name, typeof(T)) as T;
          _instance = Instantiate(prefab) as T;
          _instance.name = typeof(T).Name; // Removes (clone) naming
        }
      }
      return _instance;
    }
  }

  private void Awake()
  {
    Instantiation();
  }

  private bool Instantiation()
  {
    if(IsInstantianted)
    {
      //Debug.LogWarning("Only one " + typeof(T) + " is allowed, destroying " + gameObject.name + ".");
      DestroyImmediate(gameObject);
      return false;
    }
    _instance = FindObjectOfType(typeof(T)) as T;
    if(_persistent)
    {
      DontDestroyOnLoad(this);
    }
    return true;
  }
}