using UnityEngine;
using System.Collections;

public class TrapArrows : MonoBehaviour
{
  public GameObject projectilePrefab;
  private float reloadTime = 2f; // Segundos de tiempo entre disparos
  private bool reloading = false;

  private void Start()
  {
    StartCoroutine(Shoot());
  }

  // Comprueba si el jugador pasa por delante y dispara
  private IEnumerator Shoot()
  {
    while(true)
    {
      // Lanzamos el rayo desde la parte baja de la trampa, cerca del suelo
      Vector3 start = transform.position;
      start.y -= transform.localScale.y / 2;
      // end position based on the direction
      Vector3 forward = transform.TransformDirection(Vector3.forward) * 6f;
      Vector3 end = start + forward;
      // Lanza una linea desde la posicion de la trampa hacia adelante
      // HACER EL LINECAST EN UNA LAYER ESPECIFICA?
      RaycastHit hitInfo;
      bool hit = Physics.Linecast(start, end, out hitInfo);
      Debug.DrawRay(start, forward, Color.yellow);

      // Check hit
      if(hit)
      {
        // Dispara
        if(hitInfo.collider.gameObject.tag == "Player")
        {
          GameObject arrowInstance = Instantiate(projectilePrefab);
          arrowInstance.transform.position = transform.position;
          forward.Normalize();
          arrowInstance.transform.forward = forward;
          arrowInstance.GetComponent<Rigidbody>().AddForce(forward * 1000f);
          reloading = true;
        }
      }
      // Si acaba de disparar recarga durante un tiempo reloadTime
      if(reloading)
      {
        reloading = false;
        yield return new WaitForSeconds(reloadTime);
      }
      yield return null;
    }
  }
}
