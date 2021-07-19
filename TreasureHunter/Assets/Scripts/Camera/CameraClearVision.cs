using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * Se lanza un rayo desde la camara hacia un target y todo hace semitransparente
 * todo lo que toca mientras forme parte del escenario. 
 * Al dejar de tocar devuelve el material a su estado normal.
 */
public class CameraClearVision : MonoBehaviour
{
  private Player player;
  private Shader defaultShader;
  private Color defaultColor;
  private List<GameObject> cachedObjects;

  void Start()
  {
    cachedObjects = new List<GameObject>();
    player = GameManager.Instance.player;
    defaultShader = Shader.Find("Standard");
    defaultColor = Color.white;
  }

  void Update()
  {
    if(player != null)
    {
      Vector3 direction = player.transform.position - this.transform.position;
      float distance = Vector3.Distance(player.transform.position, this.transform.position);
      RaycastHit[] hits = Physics.RaycastAll(this.transform.position, direction, distance - 0.1f);

      // Restauramos y quitamos de la lista los objetos que ya no intersectan
      bool found = false;
      for(int i = 0; i < cachedObjects.Count; i++)
      {
        found = false;
        for(int j = 0; !found && j < hits.Length; j++)
        {
          if(cachedObjects[i] == hits[j].transform.gameObject)
          {
            found = true;
          }
        }
        if(!found)
        {
          cachedObjects[i].GetComponent<Renderer>().material.shader = defaultShader;
          cachedObjects[i].GetComponent<Renderer>().material.color = defaultColor;
          cachedObjects.RemoveAt(i);
          i--;
        }
      }

      // Recorremos la lista de objetos que intersectan y agregamos los nuevos
      for(int i = 0; i < hits.Length; i++)
      {
        if(hits[i].transform.tag == "Wall")
        {
          if(cachedObjects.Find(item => item == hits[i].transform.gameObject) == null)
          {
            Material objectMaterial = hits[i].transform.gameObject.GetComponent<Renderer>().material;
            // Cambiamos el material para que sea semitransparente
            objectMaterial.shader = Shader.Find("Particles/Additive");
            if(objectMaterial.HasProperty("_Color"))
            {
              objectMaterial.color = new Color(30, 30, 30);
            }
            cachedObjects.Add(hits[i].transform.gameObject);
          }
        }
      }
    }
  }
}
