using UnityEngine;
using System.Collections;

public class TrapAnimation : MonoBehaviour
{
  public float delayTime;
  public float startDelay;
  private Animation animationComponent;

  void Start()
  {
    this.animationComponent = GetComponent<Animation>();
    StartCoroutine(ActivateTrap());
  }

  private IEnumerator ActivateTrap()
  {
    yield return new WaitForSeconds(startDelay);
    while(true)
    {
      animationComponent.Play();
      yield return new WaitForSeconds(delayTime);
    }
  }
}
