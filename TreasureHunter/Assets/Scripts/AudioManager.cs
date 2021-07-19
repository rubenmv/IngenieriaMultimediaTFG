using UnityEngine;
using System.Collections;

public enum AudioList
{
  MusicMenu = 0,
  MusicLevel,
  PlayerAttack, // Gruñido de ataque
  PlayerHit, // Golpe del arma
  PlayerJump,
  PlayerDamage,
  Coin,
  Chest
}

public class AudioManager : MonoBehaviour
{
  // Lista de clips de audio, deben coincidir en orden con el AudioList
  public AudioClip[] clips;
  public AudioSource musicSource;
  public AudioSource fxSource;
  public static AudioManager instance = null;
  // +- 5% of original pitch, makes sounds slightly different
  public float lowPitchRange = 0.95f;
  public float highPitchRange = 1.05f;

  private bool mute = false;
  private float previousVolume;

  void Start()
  {
    previousVolume = AudioListener.volume;
  }

  // Reproduce un clip desde el source interno
  public void PlayClip(AudioList index)
  {
    musicSource.clip = clips[(int)index];
    musicSource.Play();
  }

  // Reproduce un clip desde un source externo
  public void PlayClip(AudioSource source, AudioList index, bool randomPitch)
  {
    float pitch = 1f;
    if(randomPitch)
    {
      pitch = Random.Range(lowPitchRange, highPitchRange);
    }
    source.pitch = pitch;
    source.clip = clips[(int)index];
    source.Play();
  }

  public void PlayFX(AudioList index, bool randomPitch)
  {
    float pitch = 1f;
    if(randomPitch)
    {
      pitch = Random.Range(lowPitchRange, highPitchRange);
    }
    fxSource.pitch = pitch;
    fxSource.clip = clips[(int)index];
    fxSource.Play();
  }

  public void RandomizeSfx(params AudioClip[] clips)
  {
    int randomIndex = Random.Range(0, clips.Length);
    float randomPitch = Random.Range(lowPitchRange, highPitchRange);

    musicSource.pitch = randomPitch;
    musicSource.clip = clips[randomIndex];
    musicSource.Play();
  }

  private void Update()
  {
    if(Input.GetKeyDown(KeyCode.M))
    {
      mute = !mute;
      if(mute)
      {
        AudioListener.volume = 0f;
      }
      else
      {
        AudioListener.volume = previousVolume;
      }
    }
  }

  private void OnLevelWasLoaded(int index)
  {
    if(GameManager.Instance.lastScene != index)
    {
      switch(index)
      {
        case (int)SceneName.MainMenu:
          PlayClip(AudioList.MusicMenu);
          break;
        case (int)SceneName.DungeonLevel:
          PlayClip(AudioList.MusicLevel);
          break;
      }
    }
  }
}
