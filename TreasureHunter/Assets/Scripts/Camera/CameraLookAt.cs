using UnityEngine;
using System.Collections;

public class CameraLookAt : MonoBehaviour
{
  public GameObject parent; // La camara sigue al objeto
  private bool isOrthographic = false;

  // Para bloquear ciertos movimientos dependiendo de la situacion (el jugador atacando...)
  public bool locked = false;

  // Defaults 
  private Quaternion defaultRotation;

  private const float ROTATE_FACTOR = 150f;
  private const float TRANSLATE_FACTOR = 8f;
  private const float DEFAULT_ZOFFSET = 3f;
  private const float ACCELERATION = 10f;
  private const float FRICTION = 2f;
  // La camara se mueve cuando el target sale del radio
  private const float FOLLOW_RADIUS = 2f;
  // En Orthographic el offset no hace zoom, pero si que puede cortar geometria
  // por lo que hay que ponerlo lejos
  private const float ORTHO_OFFSET = 50f;

  private float velocity = 0f; // Velocidad actual
  private Vector3 direction = Vector3.up; // Direccion de giro
  private float zOffset;
  private UIManager uiManager;

  void Start()
  {
    defaultRotation = transform.rotation;
    uiManager = GameObject.Find("UIManager").GetComponent<UIManager>();
    Reset();
  }

  public void Reset()
  {
    uiManager.LockMouse(true);
    zOffset = isOrthographic ? ORTHO_OFFSET : DEFAULT_ZOFFSET;
    if(parent != null)
    {
      transform.position = parent.transform.position - parent.transform.forward * zOffset;
    }
    transform.rotation = defaultRotation;
    locked = false;
  }

  public void SetParent(GameObject parent)
  {
    this.parent = parent;
  }
  
  public void ToggleProjection()
  {
    isOrthographic = !isOrthographic;
    if(isOrthographic)
    {
      zOffset = ORTHO_OFFSET;
      gameObject.GetComponent<Camera>().orthographic = true;
    }
  }

  void Update()
  {
    if(parent == null || locked)
    {
      return;
    }

    // Posicion con respecto al parent
    transform.position = parent.transform.position - transform.forward * zOffset;
    transform.LookAt(parent.transform);

    // ROTACION (HORIZONTAL)
    float mouseAxis = Input.GetAxis("Mouse X");
    if(!this.locked && (Input.GetKey(KeyCode.L) || mouseAxis < -0.5f))
    {// Derecha
      if(direction == Vector3.up)
      {
        velocity = 0f;
        direction = Vector3.down;
      }
      velocity += ACCELERATION;
      if(velocity > ROTATE_FACTOR)
      {
        velocity = ROTATE_FACTOR;
      }
    }
    if(!this.locked && (Input.GetKey(KeyCode.J) || mouseAxis > 0.5f))
    {// Izquierda
      if(direction == Vector3.down)
      {
        velocity = 0f;
        direction = Vector3.up;
      }
      velocity += ACCELERATION;
      if(velocity > ROTATE_FACTOR)
      {
        velocity = ROTATE_FACTOR;
      }
    }

    velocity -= FRICTION;

    if(velocity < float.Epsilon)
    {
      velocity = 0f;
    }
    transform.RotateAround(parent.transform.position, direction, velocity * Time.deltaTime);

    // Zoom en camara perspectiva
    if(!isOrthographic)
    {
      if((Input.GetAxis("Mouse ScrollWheel") > 0 || Input.GetKey(KeyCode.Plus) || Input.GetKey(KeyCode.KeypadPlus)) && zOffset >= 3f)
      {
        zOffset -= TRANSLATE_FACTOR * Time.deltaTime;
      }
      if((Input.GetAxis("Mouse ScrollWheel") < 0 || Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.KeypadMinus)) && zOffset <= 8f)
      {
        zOffset += TRANSLATE_FACTOR * Time.deltaTime;
      }
    }

    if(Input.GetKey(KeyCode.Home))
    {
      Reset();
    }
  }
}
