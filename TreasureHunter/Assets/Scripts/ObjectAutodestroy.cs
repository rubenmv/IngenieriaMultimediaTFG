using UnityEngine;
using System.Collections;

public class ObjectAutodestroy : MonoBehaviour
{
  // Algunos objetos pueden activar un efecto de partículas al morir
  public GameObject particleEffect;
  public string[] deadlyTags;
  // Si no mueren por colision, pueden morir despues de un tiempo
  public bool useTime = false;
  public float lifeTime = 0f;

  private void AutoDestroy()
  {
    Destroy(this.gameObject);
    if(particleEffect != null)
    {
      Instantiate(particleEffect, transform.position, Quaternion.identity);
    }
  }

  private IEnumerator SetTimer()
  {
    yield return new WaitForSeconds(lifeTime);
    AutoDestroy();
  }

  void Start()
  {
    if(useTime)
    {
      StartCoroutine(SetTimer());
    }
  }

  void OnCollisionEnter(Collision collision)
  {
    string cTag = collision.gameObject.tag;
    if(deadlyTags[0] == "All" && cTag != "Floor")
    {
      AutoDestroy();
    }
    else
    {
      for(int i = 0; i < deadlyTags.Length; i++)
      {
        if(cTag == deadlyTags[i])
        {
          AutoDestroy();
        }
      }
    }
  }

  void OnTriggerEnter(Collider collider)
  {
    string cTag = collider.gameObject.tag;
    if(deadlyTags[0] == "All" && cTag != "Floor")
    {
      AutoDestroy();
    }
    else
    {
      for(int i = 0; i < deadlyTags.Length; i++)
      {
        if(cTag == deadlyTags[i])
        {
          AutoDestroy();
        }
      }
    }
  }
}
