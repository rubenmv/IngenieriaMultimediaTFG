using UnityEngine;
using System.Collections;

// Estados de la IA
public enum AIState
{
  Idle,
  Patrolling,
  Chasing,
  Attacking,
  Dead
}

public class Enemy : MonoBehaviour
{
  public float speed;
  private float chaseSpeedMultiplier = 1.5f;

  public AIState state;
  public GameObject deathEffectPrefab;

  // Estado Chasing
  public float sightRadius = 3f;
  // Parametros para patrullar alrededor de punto
  public Vector3 patrolCenter;
  public float patrolRadius = 2f;
  private Vector3[] patrolPoints; // Puntos target de la zona de patrulla
  private int patrolIterator = 0; // Indica el target del array
  private Vector3 target;
  private bool reverse = false; // Recorre los targets al reves
  private bool goingRight = true;
  private Player player;
  private Coroutine routine;
  private EnemyAnimator animator;
  private SpriteRenderer sprite;

  void Awake()
  {
    animator = this.GetComponent<EnemyAnimator>();
    sprite = this.GetComponentInChildren<SpriteRenderer>();
    SetState(state);
    patrolCenter = new Vector3();
    patrolPoints = new Vector3[4];
    target = patrolPoints[0];
    CalculatePatrolPoints();
  }

  void Update()
  {
    MakeDecision();
    switch(state)
    {
      case AIState.Idle:
        Wait();
        break;
      case AIState.Patrolling:
        Patrol();
        break;
      case AIState.Chasing:
        Chase();
        break;
      case AIState.Attacking:
        Attack();
        break;
      case AIState.Dead:
        DeadAnimation();
        break;
      default:
        Wait();
        break;
    }

    ReorientSprite();
  }

  public void SetPatrolCenter(Vector3 position)
  {
    patrolCenter = position;
    CalculatePatrolPoints();
    target = patrolPoints[0];
  }

  private void CalculatePatrolPoints()
  {
    float r = patrolRadius;
    Vector3 c = patrolCenter;
    patrolPoints[0] = new Vector3(c.x + r, c.y, c.z + r);
    patrolPoints[1] = new Vector3(c.x + r, c.y, c.z - r);
    patrolPoints[2] = new Vector3(c.x - r, c.y, c.z - r);
    patrolPoints[3] = new Vector3(c.x - r, c.y, c.z + r);
  }

  private void MakeDecision()
  {
    if(state != AIState.Dead && state != AIState.Attacking)
    {
      if(player == null)
      {
        player = GameManager.Instance.player;
      }
      else
      {
        float distance = Vector3.Distance(player.transform.position, this.transform.position);
        if(state == AIState.Chasing)
        {
          if(player.isDead || distance > sightRadius)
          {
            if(routine != null)
            {
              StopCoroutine(routine);
            }
            routine = StartCoroutine(LostTarget());
          }
          else
          {
            target = player.transform.position;
          }
        }
        else
        {
          if(!player.isDead && distance <= sightRadius)
          {
            SetState(AIState.Chasing);
          }
        }
      }
    }
  }

  private IEnumerator LostTarget()
  {
    SetState(AIState.Idle);
    yield return new WaitForSeconds(1f);
    SetState(AIState.Patrolling);
    GetNextTarget();
  }

  private void SetState(AIState _state)
  {
    state = _state;
    animator.TriggerAnimation(state);
  }

  private void Wait()
  {
  }

  private void GetNextTarget()
  {
    if(reverse)
    {
      patrolIterator--;
      if(patrolIterator < 0)
      {
        patrolIterator = patrolPoints.Length - 1;
      }
    }
    else
    {
      patrolIterator++;
      if(patrolIterator >= patrolPoints.Length)
      {
        patrolIterator = 0;
      }
    }
    target = patrolPoints[patrolIterator];
    target.y = transform.position.y;
  }

  private void ReorientSprite()
  {
    Vector3 direction = target - this.transform.position;
    if(sprite.transform.rotation.y > 0 && sprite.transform.rotation.y < 180)
    {
      direction *= -1;
    }
    if(goingRight && (direction.x < 0))
    {
      // Escalamos para mirar hacia la izquierda
      sprite.transform.localScale = Vector3.Scale(sprite.transform.localScale, new Vector3(-1, 1, 1));
      goingRight = false;
    }
    else if(!goingRight && (direction.x > 0))
    {
      sprite.transform.localScale = Vector3.Scale(sprite.transform.localScale, new Vector3(-1, 1, 1));
      goingRight = true;
    }
  }

  private void MoveToTarget(float _speed)
  {
    target.y = transform.position.y; // Siempre a la altura del enemigo
    Vector3 direction = target - transform.position;
    direction.Normalize();
    transform.Translate(direction * _speed * Time.deltaTime);
  }

  private void Patrol()
  {
    MoveToTarget(speed);
    if(Vector3.Distance(transform.position, target) < 0.1f)
    {
      // Obtiene el siguiente punto de la zona de patrulla
      GetNextTarget();
    }
  }

  private void Chase()
  {
    //animator.TriggerAnimation("Walk");
    MoveToTarget(speed * chaseSpeedMultiplier);
  }

  // Animacion de ataque
  private IEnumerator Attack()
  {
    SetState(AIState.Attacking);
    float delay = animator.GetCurrentAnimationLength();
    yield return new WaitForSeconds(delay);
    SetState(AIState.Patrolling);
  }

  // Animacion de muerte, se aplica sobre el sprite ya que es el que gira segun la camara
  private void DeadAnimation()
  {
    if(sprite.transform.localScale.x > 0.1f &&
      sprite.transform.localScale.y > 0.1f)
    {
      sprite.transform.localScale -= new Vector3(0.1f, 0.1f, 0f);
    }
    sprite.transform.Rotate(new Vector3(0f, 0f, 1f), 720f * Time.deltaTime, Space.Self);
  }

  // El enemigo muere
  private IEnumerator Die()
  {
    animator.enabled = false;
    SetState(AIState.Dead);
    // Efecto de sangre
    Vector3 spawnPosition = transform.position;
    Instantiate(deathEffectPrefab, spawnPosition, deathEffectPrefab.transform.rotation);
    //Destroy();
    this.GetComponent<Rigidbody>().isKinematic = true;
    Destroy(this.GetComponentInChildren<BoxCollider>());
    Destroy(this.GetComponentInChildren<ObjectLookAtCamera>()); // En caso contrario no gira, ya que se cambia el forward
    yield return new WaitForSeconds(0.5f);
    Destroy(this.gameObject);
  }

  void OnCollisionEnter(Collision collision)
  {
    reverse = !reverse;
    string cTag = collision.gameObject.tag;
    if(cTag == "Player") // Ataque
    {
      StartCoroutine(Attack());
    }
    else if(cTag != "Floor" && cTag != "Trap") // Si choca con algun objeto busca el siguiente target
    {
      GetNextTarget();
    }
  }

  void OnTriggerStay(Collider collider)
  {
    if(collider.gameObject.tag == "Damage") // Ataque
    {
      StartCoroutine(Die());
    }
  }
}
