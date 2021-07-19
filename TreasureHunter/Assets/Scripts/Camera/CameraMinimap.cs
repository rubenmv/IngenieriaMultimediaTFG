using UnityEngine;
using System.Collections;

public class CameraMinimap : MonoBehaviour
{
  private Player target;

  // Update is called once per frame
  void Update()
  {
    if (target != null)
    {
      transform.position = new Vector3(target.transform.position.x, this.transform.position.y, target.transform.position.z);
    }
    else
    {
      target = GameManager.Instance.player;
    }
  }
}
