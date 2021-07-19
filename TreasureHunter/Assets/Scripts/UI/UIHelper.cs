using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIHelper : MonoBehaviour {
  public Text text;

  // Actualiza texto asociado desde el inspector
  // util para asociar un texto a un slider
  public void UpdateText(float value)
  {
    text.text = value.ToString();
  }
}
