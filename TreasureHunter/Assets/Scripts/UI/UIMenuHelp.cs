using UnityEngine;
using System.Collections;

public class UIMenuHelp : MonoBehaviour
{
  private Canvas canvas;

  void Start()
  {
    canvas = gameObject.GetComponent<Canvas>();
    canvas.enabled = false;
  }

  void Update()
  {
    if (Input.GetKeyDown(KeyCode.F1))
    {
      canvas.enabled = !canvas.enabled;
    }
  }
}
