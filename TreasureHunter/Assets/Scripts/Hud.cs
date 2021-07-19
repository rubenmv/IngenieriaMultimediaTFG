using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Hud : MonoBehaviour
{
  public GameObject heartEmptySprite;
  public GameObject heartFullSprite;

  private GameObject[] healthSprites;
  private GameObject healthPanel;

  private LevelManager levelManager;

  // Puntos
  private int currentPoints; // Para agregar puntos en cada actualizacion
  private int points; // Tope al que deben llegar los puntos
  private Text pointsText;
  // Timer
  private Text timerText;

  void Start()
  {
    levelManager = GameManager.Instance.levelManager;

    healthPanel = GameObject.Find("PanelHearts");
    healthSprites = new GameObject[Player.MAX_HEALTH];

    for(int i = 0; i < Player.MAX_HEALTH; i++)
    {
      healthSprites[i] = Instantiate(heartEmptySprite);
    }

    SetHealthMeter(Player.MAX_HEALTH);

    pointsText = GameObject.Find("TextPoints").GetComponent<Text>();
    timerText = GameObject.Find("TextTimer").GetComponent<Text>();
  }

  void Update()
  {
    // Puntos HUD
    if(currentPoints < points)
    {
      currentPoints++;
      pointsText.text = currentPoints.ToString();
    }
    // Temporizador
    if(timerText == null)
    {
      timerText = GameObject.Find("TextTimer").GetComponent<Text>();
    }
    timerText.text = GetFormatedTime(levelManager.timer);
  }

  private string GetFormatedTime(float time)
  {
    string str =
      Mathf.Floor(time / 60f).ToString("00") + ":" +
      Mathf.Floor(time % 60f).ToString("00");
    return str;
  }

  public void OnPointsChanged(int points)
  {
    this.points = points;
  }

  public void SetHealthMeter(int health)
  {
    RectTransform prt = healthPanel.GetComponent<RectTransform>();
    RectTransform hrt = heartFullSprite.GetComponent<RectTransform>();
    float startPosition = prt.rect.position.x + 10f;
    float spacing = hrt.rect.width;

    for(int i = 0; i < Player.MAX_HEALTH; i++)
    {
      //Debug.Log(startPosition + (spacing * i));
      Destroy(healthSprites[i].gameObject);
      if(i <= health - 1)
      {
        GameObject heartObject = Instantiate(heartFullSprite);
        heartObject.transform.SetParent(healthPanel.transform, false); // Esto lo coloca dentro del panel
        RectTransform rt = heartObject.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(startPosition + (spacing * i), 0f);
        //rt.rect.Set(startPosition + (spacing * i), prt.rect.position.y, spacing, spacing);
        healthSprites[i] = heartObject;
      }
      else
      {
        GameObject heartObject = Instantiate(heartEmptySprite);
        heartObject.transform.SetParent(healthPanel.transform, false); // Esto lo coloca dentro del panel
        RectTransform rt = heartObject.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(startPosition + (spacing * i), 0f);
        //rt.rect.Set(startPosition + (spacing * i), prt.rect.position.y, spacing, spacing);
        //rt.position = new Vector2(startPosition + (spacing * i), prt.rect.position.y);
        healthSprites[i] = heartObject;
      }
    }
  }

}
