using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Player : MonoBehaviour
{
  // Se instancia para crear una zona de daño delante del jugador cuando este ataca
  public GameObject playerAttack;
  private GameObject playerAttackInstance;

  // STATS del personaje
  public static int MAX_HEALTH = 3;
  public bool invincible = false;
  private int health;
  private int points = 0;
  // HUD
  private Hud hud;

  public bool isDead = false;
  private int hitGraceTime = 1; // Segundos invencible despues de ser golpeado
  private float maxWalkSpeed = 4.0f;
  private float maxRunSpeed = 6.0f;
  private float jumpForce = 200f;
  private bool running = false;
  private bool attacking = false;
  private float direction = -1f; // 1 derecha, -1 izquierda (en eje x)
  private bool rightDir = false; // Para girar el sprite en la direccion correcta
  private bool startJump = false; // Pasa que el FixedUpdate aplique la fuerza
  private bool onGround = true; // Indica si el jugador esta en le suelo y se puede realizar un salto

  public Sprite spriteJumpingUp;
  public Sprite spriteJumpingDown;
  private GameObject sprite;
  private SpriteRenderer spriteRenderer; // Para aplicar efectos directamente
  private Animator animator;
  private Rigidbody rigidBody; // Al que aplicar fuerza para salto

  private CameraSwitcher cameraSwitcher;
  private CameraLookAt cameraLookAt; // Camara que sigue al jugador


  // Suma los puntos y establece el tope al que tiene que llegar currentPoints
  public void AddPoints(int points)
  {
    this.points += points;
    hud.OnPointsChanged(this.points);
  }
  public int GetPoints()
  {
    return this.points;
  }

  void Start()
  {
    isDead = false;
    health = MAX_HEALTH;
    sprite = GameObject.Find("PlayerSprite");
    spriteRenderer = sprite.GetComponent<SpriteRenderer>();
    animator = sprite.GetComponent<Animator>();
    rigidBody = GetComponent<Rigidbody>();
    cameraSwitcher = Camera.main.GetComponent<CameraSwitcher>();
    cameraLookAt = Camera.main.GetComponent<CameraLookAt>();
    hud = gameObject.GetComponent<Hud>();

  }

  void Update()
  {
    float speed = 0f;
    float velocity = Mathf.Abs(rigidBody.velocity.y);
    bool setAttack = false;

    if(isDead)
    {
      return;
    }

    // INPUT
    // =====
    // Cuando esta atacando no se puede moverç
    bool blockMove = (attacking && velocity < 0.1f);
    if(!blockMove && cameraSwitcher.mode == CameraMode.LookAt)
    {
      // Si se pulsa Shift la velocidad cambia
      if(Input.GetKeyDown(KeyCode.LeftShift))
      {
        running = true;
      }
      if(Input.GetKeyUp(KeyCode.LeftShift))
      {
        running = false;
      }
      setAttack = (Input.GetKeyDown(KeyCode.Z) || Input.GetKey(KeyCode.Mouse0));
      speed = maxWalkSpeed;
      if(running)
      {
        speed = maxRunSpeed;
      }
      // SALTO. Solo cuando esta en el suelo
      if(onGround && (Input.GetKeyDown(KeyCode.Space) || Input.GetKey(KeyCode.Mouse1)))
      {
        // Activamos el flag y la fisica se hace en FixedUpdate
        startJump = true;
        onGround = false;
      }
    }

    // Activa/desactiva el modo invencible
    if(Input.GetKeyUp(KeyCode.I))
    {
      invincible = !invincible;
      if(invincible)
      {
        PlaySound(AudioList.Coin);
      }
    }

    // MOVIMIENTO
    // ==========
    // Get the horizontal and vertical axis.
    // By default they are mapped to the arrow keys.
    // The value is in the range -1 to 1
    float hAxis = Input.GetAxis("Horizontal");
    float hWalkSpeed = hAxis * speed;
    float vWalkSpeed = Input.GetAxis("Vertical") * speed;
    // Move translation along the object's x/z-axis
    transform.Translate(hWalkSpeed * Time.deltaTime, 0, 0);
    transform.Translate(0, 0, vWalkSpeed * Time.deltaTime);

    // On 0 movement won't flip sprite
    if(hAxis != 0)
    {
      bool goingRight = false;
      direction = -1f;
      if(Input.GetAxis("Horizontal") > 0)
      {
        goingRight = true;
        direction = 1f;
      }

      // Change direction of the sprite
      if(goingRight != rightDir)
      {
        transform.localScale = Vector3.Scale(transform.localScale, new Vector3(-1, 1, 1));
        rightDir = goingRight;
      }
    }

    animator.enabled = true; 
    // ANIMACIONES
    // ===========
    if(!setAttack && velocity > 0.1f)
    {
      animator.enabled = false;
      if(rigidBody.velocity.y > 0) // Subiendo
      {
        spriteRenderer.sprite = spriteJumpingUp;
      }
      else // Bajando
      {
        spriteRenderer.sprite = spriteJumpingDown;
      }
    }
    // Threshold speed for walk/idle animation
    else if(!animator.GetCurrentAnimatorStateInfo(0).IsName("PlayerBilboAttack"))
    {
      float tSpeed = maxWalkSpeed * 0.5f;
      if(Mathf.Abs(vWalkSpeed) > tSpeed || Mathf.Abs(hWalkSpeed) > tSpeed)
      {
        if(running)
        {
          animator.Play("PlayerBilboRun");
        }
        else
        {
          animator.Play("PlayerBilboWalk");
        }
      }
      else
      {
        animator.Play("PlayerBilboIdle");
      }
    }
    // Attack
    if(setAttack)
    {
      animator.Play("PlayerBilboAttack");
      if(!attacking)
      {
        StartCoroutine(Attack());
      }
    }
    // Recoloca la caja de golpe al lado derecho del jugador
    if(playerAttackInstance != null)
    {
      playerAttackInstance.transform.position = this.transform.position + (direction * this.transform.right * 0.8f);
    }
  }

  private void PlaySound(AudioList sound)
  {
    GameManager.Instance.audioManager.PlayClip(this.GetComponent<AudioSource>(), sound, true);
  }

  private IEnumerator Attack()
  {
    PlaySound(AudioList.PlayerAttack);
    attacking = true;
    if(Mathf.Abs(rigidBody.velocity.y) < 0.1f)
    {// Solo bloquea la camara cuando se ataca desde el suelo
      cameraLookAt.locked = true;
    }
    // El length a una animacion no cambia al modificar su speed en el editor
    // por lo que tenemos que tomar el valor de length y dividirlo entre la speed
    // que le hemos dado a la animacion en el editor
    float animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
    float animationTime = animationLength / 1.5f;
    yield return new WaitForSeconds(animationTime * 0.2f);
    // Instanciamos la caja de daño al lado del Player
    Vector3 start = this.transform.position;
    start.y -= 0.1f;
    Vector3 end = start + direction * this.transform.right * 0.5f;
    playerAttackInstance = (GameObject)Instantiate(playerAttack, start, this.transform.rotation);
    //playerAttackInstance.transform.localScale = new Vector3(playerAttack.transform.localScale.x, playerAttack.transform.localScale.y, playerAttack.transform.localScale.z);
    Vector3 position = start;
    while(Vector3.Distance(position, end) > 0.1f)
    {
      position += (direction * this.transform.right * 0.1f);
      playerAttackInstance.transform.position = position;
      // Debug.Log(position);
      yield return null;
    }
    //PlaySound(AudioList.PlayerHit);
    yield return new WaitForSeconds(animationTime * 0.6f);
    Destroy(playerAttackInstance);
    yield return new WaitForSeconds(animationTime * 0.2f);
    attacking = false;

    if(!isDead)
    {
      cameraLookAt.locked = false;
    }
  }

  void FixedUpdate()
  {
    if(startJump)
    {
      startJump = false;
      rigidBody.velocity *= 0;
      rigidBody.AddForce(Vector3.up * jumpForce);
      PlaySound(AudioList.PlayerJump);
    }
  }

  void OnCollisionEnter(Collision collision)
  {
    onGround = true;
    string cTag = collision.gameObject.tag;

    if(cTag == "Enemy" && !invincible)
    {
      StartCoroutine(OnPlayerDamaged());
    }
    else if(cTag == "Exit")
    {
      GameManager.Instance.FinishLevel();
    }
    else if(cTag == "Wall")
    {
      Vector3 direction = collision.gameObject.transform.position - this.transform.position;
      rigidBody.AddForce(direction);
    }
  }
 
  private void OnTriggerEnter(Collider collider)
  {
    string cTag = collider.gameObject.tag;
    if(cTag == "Enemy" && !invincible)
    {
      StartCoroutine(OnPlayerDamaged());
    }
    else if(cTag == "Coin")
    {
      PlaySound(AudioList.Coin);
      AddPoints((int)ItemPoints.COIN);
    }
    else if(cTag == "Potion")
    {
      SetHealth(MAX_HEALTH);
    }
  }

  // Alterna el SpriteRenderer para producir efecto
  void BlinkSprite()
  {
    spriteRenderer.enabled = !spriteRenderer.enabled;
  }

  private void SetHealth(int health)
  {
    this.health = health;
    hud.SetHealthMeter(this.health);
  }

  // Espera un tiempo de gracia durante el cual el 
  IEnumerator OnPlayerDamaged()
  {
    PlaySound(AudioList.PlayerDamage);
    SetHealth(health - 1);
    if(health <= 0)
    {
      isDead = true;
      cameraLookAt.locked = true;
      GetComponent<Rigidbody>().isKinematic = true;
      GetComponent<BoxCollider>().enabled = false;
      animator.Play("PlayerBilboDead");
      yield return new WaitForSeconds(3f); // Dejamos tiempo para que se vea la animacion
      GameManager.Instance.GameOver();
    }
    else
    {
      invincible = true;
      // Activa el parpadeo del sprite durante el tiempo de gracia
      // Parpadeo de sprite cuando es invencible
      InvokeRepeating("BlinkSprite", 0f, 0.1f);
      yield return new WaitForSeconds(hitGraceTime);
      // Restaura
      CancelInvoke(); // ATENCION: Detiene todos los InvokeRepeating
      spriteRenderer.enabled = true;
      invincible = false;
    }
  }
}
