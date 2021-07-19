using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LevelManager : MonoBehaviour
{
  public int MIN_DUNGEON_DIM = 50;
  public GameObject prefDungeonGenerator;
  public GameObject dungeonGenerator;
  
  // Timer para los minutos:segundos:milisegundos
  [HideInInspector]
  public float
    timer;

  public void Init()
  {
    int level = GameManager.Instance.GetLevel();
    dungeonGenerator = Instantiate(prefDungeonGenerator);

    int width = Random.Range(MIN_DUNGEON_DIM + (level * 5), MIN_DUNGEON_DIM + (level * 5) + 5);
    int height = Random.Range(MIN_DUNGEON_DIM + (level * 5), MIN_DUNGEON_DIM + (level * 5) + 5);
    dungeonGenerator.GetComponent<DungeonGenerator>().GenerateDungeon(width, height);

    StartCoroutine(StartLevel());
  }

  private IEnumerator StartLevel()
  {
    // Fade screen
    CanvasGroup canvas = GameObject.Find("CanvasFade").GetComponentInChildren<CanvasGroup>();

    while(canvas.alpha > 0)
    {
      canvas.alpha -= 0.01f;
      yield return null;
    }

    Destroy(canvas.gameObject);
    // Comienza el temporizador
    timer = 0f;
  }

  void Update()
  {
    timer += Time.deltaTime;
  }

  public void SaveToFile()
  {
    if(dungeonGenerator != null)
    {
      dungeonGenerator.GetComponent<DungeonGenerator>().SaveToFile();
    }
  }

  public void Cleanup()
  {
    DestroyImmediate(dungeonGenerator);
  }
}
