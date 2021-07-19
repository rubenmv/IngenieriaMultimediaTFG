using UnityEngine;
using System.Collections;

public class EnemyAnimator : MonoBehaviour
{
  private Animator animator;

  // Use this for initialization
  void Start()
  {
    animator = this.GetComponentInChildren<Animator>();
  }

  // Activa una animacion mediante trigger
  public void TriggerAnimation(AIState state)
  {
    string stateName = "Idle";
    switch (state)
    {
      case AIState.Patrolling:
      case AIState.Chasing:
        stateName = "Walk";
        break;
      case AIState.Attacking:
        stateName = "Attack";
        break;
    }
    this.GetComponentInChildren<Animator>().SetTrigger(stateName);
  }

  public float GetCurrentAnimationLength()
  {
    return animator.GetCurrentAnimatorStateInfo(0).length;
  }
}
