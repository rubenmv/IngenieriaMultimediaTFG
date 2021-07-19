using UnityEngine;
using System.Collections;

public class SceneTransition : MonoBehaviour
{
  public float startTime = 1f; // Tiempo(seg) hasta que empieza la animacion
  public float delayTime = 2f; // Tiempo(seg) que se espera antes de volver a reducir el alpha
  public SceneName nextScene = SceneName.MainMenu; // Escena a cargar al finalizar la transicion

  private CanvasGroup canvas;
  private float changeFactor = 0.01f; // Cantidad que sube o baja el alpha en cada frame

  // Use this for initialization
  void Start()
  {
    canvas = gameObject.GetComponent<CanvasGroup>();
    canvas.alpha = 0.01f; // Asi podemos terminar cuando sea 0 o menos
    StartCoroutine(AnimateAlpha());
  }

  private IEnumerator AnimateAlpha()
  {
    // Esperamos un tiempo inicial antes de que empiece a aparecer
    yield return new WaitForSeconds(startTime);

    WaitForSeconds delay = new WaitForSeconds(delayTime);
    // Modificamos el alpha mientras este en proceso (> 0)
    while(canvas.alpha > 0f && !Input.anyKeyDown)
    {
      canvas.alpha += changeFactor;
      // Si llega a 1 invertimos el proceso despues de una pausa
      if(canvas.alpha >= 1f)
      {
        canvas.alpha = 1f;
        yield return delay;
        // Continuamos hacia abajo
        changeFactor *= -1;
      }
      yield return null;
    }
    // Al finalizar cargamos la escena de menu principal
    GameManager.Instance.LoadScene((int)nextScene);
  }
}
