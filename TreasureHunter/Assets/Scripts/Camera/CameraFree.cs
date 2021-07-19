using UnityEngine;
using System.Collections;

public class CameraFree : MonoBehaviour
{
  public float cameraSensitivity = 200f;
  public float climbSpeed = 8f;
  public float normalMoveSpeed = 20f;
  public float slowMoveFactor = 0.25f;
  public float fastMoveFactor = 3f;
  private float orthoZoomSpeed = 2f;

  private float rotationX = 0.0f;
  private float rotationY = 0.0f;

  private Camera cameraInstance;

  void Start()
  {
    rotationX = -90f;
    rotationY = -80f;
    transform.localRotation = Quaternion.AngleAxis(rotationX, Vector3.up);
    transform.localRotation *= Quaternion.AngleAxis(rotationY, Vector3.left);
    cameraInstance = this.GetComponent<Camera>();
  }

  void Update()
  {
    // Con boton derecho de raton orientamos la camara
    if(Input.GetKey(KeyCode.Mouse1))
    {
      rotationX += Input.GetAxis("Mouse X") * cameraSensitivity * Time.deltaTime;
      rotationY += Input.GetAxis("Mouse Y") * cameraSensitivity * Time.deltaTime;
      rotationY = Mathf.Clamp(rotationY, -90f, 90f);

      transform.localRotation = Quaternion.AngleAxis(rotationX, Vector3.up);
      transform.localRotation *= Quaternion.AngleAxis(rotationY, Vector3.left);
    }

    float moveFactor = 0.25f;

    // Shift aumenta la velocidad de movimiento
    if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
    {
      moveFactor = fastMoveFactor;
    }
    // Control disminuye la velocidad de movimiento
    else if(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
    {
      moveFactor = slowMoveFactor;
    }

    if(cameraInstance.orthographic)
    {
      cameraInstance.orthographicSize -= Input.GetAxis("Vertical") * moveFactor * orthoZoomSpeed * Time.deltaTime;
    }
    else
    {
      transform.position += transform.forward * normalMoveSpeed * moveFactor * Input.GetAxis("Vertical") * Time.deltaTime;
    }
    transform.position += transform.right * normalMoveSpeed * moveFactor * Input.GetAxis("Horizontal") * Time.deltaTime;

    // Mueve la camara en direccion superior con respecto a ella misma
    if(Input.GetKey(KeyCode.Q))
    {
      transform.position += transform.up * climbSpeed * moveFactor * Time.deltaTime;
    }
    // Mueve la camara en direccion inferior con respecto a ella misma
    if(Input.GetKey(KeyCode.E))
    {
      transform.position -= transform.up * climbSpeed * moveFactor * Time.deltaTime;
    }
  }
}