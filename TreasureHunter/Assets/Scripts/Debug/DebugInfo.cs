using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DebugInfo : MonoBehaviour
{
  public Text prefabDebugText;

  private float fpsCount;
  private bool active = false;
  private GameObject panelDebugInfo = null;
  private Text fpsText;
  private float yPosition; // Ultima posicion

  void Awake()
  {
    // Recogemos la instancia antes de ocultarlo
    panelDebugInfo = GameObject.Find("PanelDebugInfo");
    panelDebugInfo.SetActive(false);
    fpsText = Instantiate(prefabDebugText);
    fpsText.transform.SetParent(panelDebugInfo.transform, false);
    RectTransform rt = fpsText.GetComponent<RectTransform>();
    yPosition = rt.position.y;
    StartCoroutine(UpdateFPS());
  }

  private IEnumerator UpdateFPS()
  {
    while(true)
    {
      if(Time.timeScale != 0f)
      {
        yield return new WaitForSeconds(0.1f);
        fpsCount = (1 / Time.deltaTime);
        fpsText.text = "FPS :" + (Mathf.Round(fpsCount));
      }
      else
      {
        fpsText.text = "Pause";
      }
      yield return new WaitForSeconds(0.5f);
    }
  }

  public void AddInfo(string content)
  {
    Text newInfo = Instantiate(prefabDebugText);
    newInfo.transform.SetParent(panelDebugInfo.transform, false);
    RectTransform rt = newInfo.GetComponent<RectTransform>();
    Vector3 pos = rt.position;
    pos.y -= (yPosition + 20f);
    yPosition = pos.y;
    rt.position = pos;
    newInfo.text = content;
  }

  void Update()
  {
    if(Input.GetKeyDown(KeyCode.F2))
    {
      active = !active;
      panelDebugInfo.SetActive(active);
    }
  }
}
