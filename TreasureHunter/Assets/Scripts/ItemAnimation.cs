using UnityEngine;
using System.Collections;

public class ItemAnimation : MonoBehaviour
{
  private float speed = 5f;

  private IEnumerator Die(GameObject targetObject)
  {
    this.GetComponent<BoxCollider>().enabled = false;

    Vector3 target = transform.position + Vector3.up * 1.5f;

    while(transform.position.y < target.y)
    {
      transform.Translate(Vector3.up * speed * Time.deltaTime, Space.World);
      yield return null;
    }

    target = targetObject.transform.position;
    while(Vector3.Distance(target, transform.position) > 0.4f)
    {
      if(transform.localScale.x > 0.1f)
      {
        transform.localScale -= new Vector3(0.1f, 0.1f, 0.1f);
      }
      Vector3 direction = target - transform.position;
      direction.Normalize();
      transform.Translate(direction * speed * 3f * Time.deltaTime, Space.World);
      target = targetObject.transform.position;
      yield return null;
    }

    Destroy(gameObject);
    yield return null;
  }

  private void OnTriggerEnter(Collider collider)
  {
    if(collider.gameObject.tag == "Player")
    {
      StartCoroutine(Die(collider.gameObject));
    }
  }
}
