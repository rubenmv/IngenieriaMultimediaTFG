using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TestGenerators : MonoBehaviour
{
  // Generadores
  public GeneratorGrowingTree prefabGeneratorGT; // Growing Tree
  private GeneratorGrowingTree instanceGeneratorGT;
  public GeneratorCA prefabGeneratorCA; // Cellular Automata
  private GeneratorCA instanceGeneratorCA;
  public GeneratorBSP prefabGeneratorBSP; // BSP Tree
  private GeneratorBSP instanceGeneratorBSP;

  private Button stepButton;

  // Valores por defecto de camara segun el test
  private GameObject cameraInstance;
  private Vector3[] cameraDefaults = { 
                                       new Vector3(-1f, 15f, -2f), // Posicion para Growing Tree
                                       new Vector3(90f, 0f, 0f), // Rotacion para Growing Tree
                                       new Vector3(20f, 55f, 12f), // Posicion para el resto
                                       new Vector3(90f, 0f, 0f) // Rotacion para el resto
                                     };

  // OPCIONES DE LOS TEST
  private int selectedAlg = 0;
  // Los distintos generadores para los test
  public GameObject settingsSize;
  public int DUNGEON_WIDTH;
  public int DUNGEON_HEIGHT;

  // Growing Tree
  public GameObject settingsGT;
  private float delay = 0f;
  private bool random = false;

  // Automata celular
  public GameObject settingsCA; // Instancia del panel de opciones
  private int cellularAutomataPasses = 1;
  private int wallProbability = 40;
  private bool useSeed = false;
  private int seed = 0;

  // BSP Tree
  public GameObject settingsBSP;

  public GameObject settingsSeed;

  // Setters para los sliders de los test
  public void SetDelay(float value)
  {
    delay = value;
  }

  public void SetRandom(bool value)
  {
    random = value;
  }

  public void SetSeed(bool value)
  {
    useSeed = value;
  }

  public void SetStepButton(bool value)
  {
    stepButton.interactable = value;
  }

  public void SetDungeonWidth(float value)
  {
    DUNGEON_WIDTH = (int)value;
  }

  public void SetDungeonHeight(float value)
  {
    DUNGEON_HEIGHT = (int)value;
  }

  public void SetCAPasses(float value)
  {
    cellularAutomataPasses = (int)value;
  }

  public void SetCAProbability(float value)
  {
    wallProbability = (int)value;
  }

  public void SetBSPRoomSize(float value)
  {
    GeneratorBSP.ROOM_SIZE = value;
  }

  private void Start()
  {
    cameraInstance = Camera.main.gameObject;
    stepButton = GameObject.Find("ButtonStep").GetComponent<Button>();
    settingsGT.SetActive(true);
    settingsCA.SetActive(false);
    settingsBSP.SetActive(false);
    settingsSeed.SetActive(false);
  }

  private void Update()
  {
    if(Input.GetKeyDown(KeyCode.P))
    {
      OnStep();
    }
  }

  // Activa las opciones del algoritmo seleccionado
  public void OnAlgSelected(float selection)
  {
    selectedAlg = (int)selection;

    settingsSize.SetActive(false);
    settingsGT.SetActive(false);
    settingsCA.SetActive(false);
    settingsBSP.SetActive(false);
    settingsSeed.SetActive(false);
    switch(selectedAlg)
    {
      case 0: // Growing Tree
        settingsGT.SetActive(true);
        break;
      case 1: // Cellular Automata
        settingsCA.SetActive(true);
        settingsSize.SetActive(true);
        settingsSeed.SetActive(true);
        break;
      case 2: // BSP Tree
        settingsBSP.SetActive(true);
        settingsSize.SetActive(true);
        settingsSeed.SetActive(true);
        break;
    }
  }
  // Boton de comenzar a ejecutar el algoritmo
  public void OnRun()
  {
    switch(selectedAlg)
    {
      case 0: // Growing Tree
        stepButton.interactable = false;
        SetCamera();
        GenerateDungeonGrowingTree();
        break;
      case 1: // Cellular Automata
        stepButton.interactable = true;
        SetCamera();
        GenerateDungeonCA();
        break;
      case 2: // BSP Tree
        stepButton.interactable = true;
        SetCamera();
        GenerateDungeonBSP();
        break;
    }
  }

  private void SetCamera()
  {
    switch(selectedAlg)
    {
      case 0:
        cameraInstance.transform.position = cameraDefaults[0];
        cameraInstance.GetComponent<Camera>().orthographicSize = 8;
        cameraInstance.transform.localEulerAngles = cameraDefaults[1];
        break;
      case 1:
      case 2:
        cameraInstance.transform.position = cameraDefaults[2];
        cameraInstance.GetComponent<Camera>().orthographicSize = 40;
        cameraInstance.transform.localEulerAngles = cameraDefaults[3];
        break;
    }
  }

  public void OnStep()
  {
    switch(selectedAlg)
    {
      case 1: // Cellular Automata
        instanceGeneratorCA.doNextStep = true;
        break;
      case 2: // BSP Tree
        instanceGeneratorBSP.doNextStep = true;
        break;
    }
  }

  public void Cleanup()
  {
    if(instanceGeneratorGT != null)
    {
      instanceGeneratorGT.Cleanup();
      Destroy(instanceGeneratorGT.gameObject);
    }

    if(instanceGeneratorCA != null)
    {
      instanceGeneratorCA.Cleanup();
      Destroy(instanceGeneratorCA.gameObject);
    }

    if(instanceGeneratorBSP != null)
    {
      instanceGeneratorBSP.Cleanup();
      Destroy(instanceGeneratorBSP.gameObject);
    }
  }

  // Genera la mazmorra usando el algoritmo de Growing Tree
  private void GenerateDungeonGrowingTree()
  {
    Cleanup();
    instanceGeneratorGT = Instantiate(prefabGeneratorGT);
    instanceGeneratorGT.name = "GeneratorGrowingTree";
    instanceGeneratorGT.Generate(delay, random);
  }

  private void SetSeed()
  {
    if(useSeed)
    {
      Text input = GameObject.Find("TextSeed").GetComponent<Text>();
      bool result = int.TryParse(input.text, out seed);
      if(!result) // No se puede parsear el entero, cancelamos la semilla
      {
        Debug.Log("Unable to convert input string to int");
        input.text = "";
        seed = -1;
        useSeed = false;
      }
    }
    else
    {
      seed = -1;
    }
  }

  // Genera la mazmorra usando el algoritmo de Automata Celular
  private void GenerateDungeonCA()
  {
    Cleanup();
    instanceGeneratorCA = Instantiate(prefabGeneratorCA);
    instanceGeneratorCA.name = "GeneratorCA";
    SetSeed();
    instanceGeneratorCA.Generate(DUNGEON_WIDTH, DUNGEON_HEIGHT, cellularAutomataPasses, wallProbability, seed);
  }

  // Genera la mazmorra usando el algoritmo de BSP Tree
  private void GenerateDungeonBSP()
  {
    Cleanup();
    instanceGeneratorBSP = Instantiate(prefabGeneratorBSP);
    instanceGeneratorBSP.name = "GeneratorBSP";
    SetSeed();
    instanceGeneratorBSP.Generate(DUNGEON_WIDTH, DUNGEON_HEIGHT, seed);
  }
}
