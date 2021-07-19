using UnityEngine;
using System.Collections;

public class TrapRollingRock : MonoBehaviour
{
  public GameObject rockObject;
  private float rollingForce = 1500f;
  private bool activated = false;

  private void TriggerRock()
  {
    // La roca se lanza en direccion al trigger
    rockObject.GetComponent<Rigidbody>().isKinematic = false;
    rockObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
    Vector3 direction = transform.position - rockObject.transform.position;
    rockObject.GetComponent<Rigidbody>().AddForce(direction * rollingForce);
  }

  void OnCollisionEnter(Collision collision)
  {
    if (rockObject == null)
    {
      Destroy(this.transform.parent.gameObject);
      return;
    }
    if (!activated && collision.gameObject.tag == "Player")
    {

      activated = false;
      TriggerRock();
      gameObject.SetActive(false);
    }
  }
}
