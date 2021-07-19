using UnityEngine;
using System.Collections;

public class Chest : MonoBehaviour
{
  public GameObject particlesGemsPrefab;
  public GameObject particlesDiamondsPrefab;

  private IEnumerator OpenChest()
  {
    Animation openAnimation = GetComponentInChildren<Animation>();
    if(openAnimation != null)
    {
      openAnimation.Play();
    }
    yield return new WaitForSeconds(0.6f);

    // Activa el efecto de particulas y comienza a sumar puntos

    Instantiate(particlesGemsPrefab, transform.position, particlesGemsPrefab.transform.rotation);
    Instantiate(particlesDiamondsPrefab, transform.position, particlesGemsPrefab.transform.rotation);
    GameManager.Instance.audioManager.PlayFX(AudioList.Chest, true);
    Destroy(gameObject.GetComponent<Chest>());
  }

  void OnTriggerEnter(Collider collider)
  {
    if(collider.gameObject.tag == "Damage")
    {
      StartCoroutine(OpenChest());
      GameManager.Instance.player.AddPoints((int)ItemPoints.CHEST);
    }
  }
}
