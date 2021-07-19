using UnityEngine;
using System.Collections;

public class PauseMenu : MonoBehaviour
{
  public GameObject pauseCanvas;

  // Update is called once per frame
  void Update()
  {
    if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
    {
      TogglePause();
    }
  }

  public void TogglePause()
  {
    pauseCanvas.SetActive(!pauseCanvas.activeSelf);
    Time.timeScale = 1.0f - Time.timeScale;
  }
}
