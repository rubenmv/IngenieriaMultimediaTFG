using UnityEngine;
using System.Collections;

/**
 * Script que hace que el objeto siempre mire hacia la camara.
 * Generalmente se usa para sprites en entornos 3D
 * */
public class ObjectLookAtCamera : MonoBehaviour
{
  private GameObject cameraInstance;

  void Start()
  {
    cameraInstance = Camera.main.gameObject;
  }

  void Update()
  {
    Vector3 forwardDirection = cameraInstance.transform.forward;
    forwardDirection.y = 0; // No queremos que miren hacia arriba/abajo
    if(forwardDirection != Vector3.zero)
    {
      transform.forward = forwardDirection;
    }
  }
}
