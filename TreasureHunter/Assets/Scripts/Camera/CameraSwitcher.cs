using UnityEngine;
using System.Collections;

public enum CameraMode
{
  None,
  Free,
  LookAt
}

public class CameraSwitcher : MonoBehaviour
{
  public bool locked = false;
  public CameraMode mode = CameraMode.None;

  private CameraLookAt cameraLookAt;
  private CameraFree cameraFree;
  private CameraClearVision cameraClearVision;

  // Use this for initialization
  void Start()
  {
    mode = CameraMode.LookAt;
    cameraLookAt = GetComponent<CameraLookAt>();
    cameraFree = GetComponent<CameraFree>();
    cameraClearVision = GetComponent<CameraClearVision>();
  }

  // Update is called once per frame
  void Update()
  {
    if(Input.GetKeyDown(KeyCode.C)) // Camara lookat <--> libre
    {
      cameraLookAt.enabled = !cameraLookAt.enabled;
      cameraClearVision.enabled = !cameraClearVision.enabled;
      cameraFree.enabled = !cameraFree.enabled;

      if(cameraLookAt.enabled)
      {
        cameraLookAt.Reset();
        mode = CameraMode.LookAt;
      }
      else
      {
        mode = CameraMode.Free;
      }
    }
    else if(Input.GetKeyDown(KeyCode.O))// Camara perspectiva <--> ortho
    {
      cameraLookAt.ToggleProjection();
    }
  }
}
