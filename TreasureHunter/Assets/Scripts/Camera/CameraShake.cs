using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{

  public float shakeAmount = 0.25f;
  public float decreaseFactor = 1.0f;

  private Vector3 cameraPos;
  private float shakeValue = 0.0f;

  void Update()
  {
    // Comprueba si la camara esta en modo de vibracion
    if(this.shakeValue > 0f)
    {
      // Vibra la camara
      transform.localPosition = transform.localPosition + (Random.insideUnitSphere * this.shakeAmount * this.shakeValue);

      // Vamos reduciendo poco a poco
      shakeValue -= Time.deltaTime * decreaseFactor;

      // Si se ha parado entonces lo detenemos completamente
      if(shakeValue <= 0f)
      {
        shakeValue = 0f;
        this.transform.localPosition = this.cameraPos;
      }
    }
  }

  // Activa la vibracion de la camara, si no estaba ya activa
  public void Shake(float amount)
  {
    if(shakeValue <= 0.0f)
    {
      cameraPos = transform.position;
    }
    shakeValue = amount;
  }
}
