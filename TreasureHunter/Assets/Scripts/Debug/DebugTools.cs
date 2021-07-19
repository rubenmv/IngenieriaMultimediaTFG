using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Rendering;
using System;
using System.IO;

public class DebugTools : MonoBehaviour
{
  public string METRICS_PATH = "Metrics/";

  // Instancia publica del singleton
  public static DebugTools instance = null;

  void Awake()
  {
    // Inicializacion de la instancia del singleton
    if(instance == null)
    {
      instance = this;
    }
    else if(instance != this)
    {
      Destroy(gameObject);
    }
    DontDestroyOnLoad(gameObject);
  }

  void Update()
  {
    if(Input.GetKeyDown(KeyCode.R))
    { // Restart level
      GameManager.Instance.LoadScene(Application.loadedLevel);
    }
    if(Input.GetKeyDown(KeyCode.N))
    { // Next level
      GameManager.Instance.FinishLevel();
    }
    else if(Input.GetKeyDown(KeyCode.F3)) // Save level to file
    {
      GameManager.Instance.levelManager.SaveToFile();
    }
  }

  // Escribe el contenido de una cadena a un fichero indicado
  public void WriteToFile(string fileName, string content)
  {
    try
    {
      string directory = fileName.Remove(fileName.LastIndexOf(@"/"));
      Directory.CreateDirectory(directory);
      using(System.IO.StreamWriter file = new System.IO.StreamWriter(fileName))
      {
        file.Write(content);
      }
    }
    catch(Exception ex)
    {
      Debug.Log(ex.ToString());
    }
  }
}

