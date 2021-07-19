using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GeneratorBSP : MonoBehaviour
{
  public static float ROOM_SIZE = 15f;

  // Los distintos generadores para los test
  public int DUNGEON_WIDTH;
  public int DUNGEON_HEIGHT;
  public GameObject baseRoom;
  public GameObject floorTile;
  public GameObject wallTile;
  // Plano/rejilla sobre el que realizar las subdivisiones
  public t_Grid levelGrid;

  private Coroutine routine;
  private int roomID = 0;
  private t_BSPTree bspTree;

  private int seed;
  public bool doNextStep = false; // Se activara por tecla o por boton

  private List<Object> tileObjectList;

  void Start()
  {
    tileObjectList = new List<Object>();
  }

  // Setters para los sliders de los test
  public void SetDungeonWidth(float value)
  {
    DUNGEON_WIDTH = (int)value;
  }

  public void SetDungeonHeight(float value)
  {
    DUNGEON_HEIGHT = (int)value;
  }

  // Genera la mazmorra usando el algoritmo de BSP Tree
  public void Generate(int width, int height, int seed)
  {
    DUNGEON_WIDTH = width;
    DUNGEON_HEIGHT = height;
    this.seed = seed;
    if(seed >= 0)
    {
      Random.seed = this.seed;
    }
    // Creamos el cubo inicial
    GameObject rootArea = GameObject.CreatePrimitive(PrimitiveType.Cube);
    rootArea.transform.localScale = new Vector3(DUNGEON_WIDTH, 1, DUNGEON_HEIGHT);
    rootArea.tag = "GenSection";
    rootArea.transform.position = new Vector3(transform.position.x + rootArea.transform.localScale.x / 2,
            transform.position.y,
            transform.position.z + rootArea.transform.localScale.z / 2);

    levelGrid = new t_Grid((int)rootArea.transform.localScale.x, (int)rootArea.transform.localScale.z);

    for(int i = 0; i < levelGrid.GetWidth(); i++)
    {
      for(int j = 0; j < levelGrid.GetHeight(); j++)
      {
        levelGrid.SetTile(i, j, 0);
      }
    }
    // Crea el arbol y agrega el primer nodo raiz
    bspTree = new t_BSPTree(rootArea);

    // Crea los nodos del arbol subdividiendo el plano inicial
    // finalmente lanza la creacion de las habitaciones, pasillos y paredes de la mazmorra
    routine = StartCoroutine(CreateDungeon());
  }

  private IEnumerator CreateDungeon()
  {
    int step = 0;
    while(step < 7)
    {
      if(doNextStep)
      {
        Split(bspTree.Root);
        step++;
        doNextStep = false;
      }
      yield return null;
    }

    // Crea las habitaciones
    CreateRooms(bspTree.Root);

    EraseLevelObjects();
    DrawLevel();

    while(!doNextStep)
    {
      yield return null;
    }
    doNextStep = false;
    // Conecta las habitaciones con pasillos
    ConnectRooms(bspTree.Root);

    EraseLevelObjects();
    DrawLevel();

    while(!doNextStep)
    {
      yield return null;
    }
    doNextStep = false;
    // Limpieza mediante automata celular
    CleanLevelGeneration();
    EraseLevelObjects();
    DrawLevel();

    GameObject.Find("TestGenerators").GetComponent<TestGenerators>().SetStepButton(false);
  }

  // Limpia los objetos creados
  public void Cleanup()
  {
    if(routine != null)
    {
      StopCoroutine(routine);
    }
    bspTree = null;
    // Eliminamos los cubos base "GenSection"
    GameObject[] objectList = GameObject.FindGameObjectsWithTag("GenSection");
    for(int i = 0; i < objectList.Length; i++)
    {
      Destroy(objectList[i].gameObject);
    }

    EraseLevelObjects();

    objectList = GameObject.FindGameObjectsWithTag("Digger");
    for(int i = 0; i < objectList.Length; i++)
    {
      Destroy(objectList[i].gameObject);
    }
    objectList = GameObject.FindGameObjectsWithTag("BaseRoom");
    for(int i = 0; i < objectList.Length; i++)
    {
      Destroy(objectList[i].gameObject);
    }
    GameObject floor = GameObject.Find("Floor");
    if(floor != null)
    {
      Destroy(floor.gameObject);
    }
  }

  //split the tree
  public void Split(t_BSPNode pNode)
  {
    if(pNode.GetLeftNode() != null)
    {
      Split(pNode.GetLeftNode());
    }
    else
    {
      pNode.Cut();
      return;
    }

    if(pNode.GetLeftNode() != null)
    {
      Split(pNode.GetRightNode());
    }
  }

  public t_Grid GetGrid()
  {
    return levelGrid;
  }

  public void SetTile(int x, int y, int value)
  {
    levelGrid.SetTile(x, y, value);
  }

  private void AddRoom(t_BSPNode pNode)
  {

    GameObject aObj = pNode.GetCube();

    GameObject aRoom = (GameObject)Instantiate(baseRoom, aObj.transform.position, Quaternion.identity);
    aRoom.transform.localScale = new Vector3(
            (int)(Random.Range(10, aObj.transform.localScale.x - 5)),
            aRoom.transform.localScale.y,
            (int)(Random.Range(10, aObj.transform.localScale.z - 5)));
    aRoom.GetComponent<t_RoomCreator>().Setup();
    aRoom.GetComponent<t_RoomCreator>().SetID(roomID);
    aRoom.GetComponent<t_RoomCreator>().SetParentNode(pNode);
    pNode.SetRoom(aRoom);
    roomID++;
  }

  private void CreateRooms(t_BSPNode pNode)
  {
    if(pNode.GetLeftNode() != null)
    {
      CreateRooms(pNode.GetLeftNode());
    }
    else
    {
      AddRoom(pNode);
      return;
    }

    if(pNode.GetRightNode() != null)
    {
      CreateRooms(pNode.GetRightNode());
    }
  }

  // Crea los pasillos para conectar las habitaciones
  // Recorre recursivamente el arbol
  private void ConnectRooms(t_BSPNode pNode)
  {
    // Recorrido por los nodos de la parte izquierda
    if(pNode.GetLeftNode() != null)
    {
      ConnectRooms(pNode.GetLeftNode());

      if(pNode.GetRoom() != null)
      {
        pNode.GetRoom().GetComponent<t_RoomCreator>().Connect();
        return;
      }

    }
    else
    {
      if(pNode.GetRoom() != null)
      {
        pNode.GetRoom().GetComponent<t_RoomCreator>().Connect();
        return;
      }
    }
    // Recorrido por los nodos de la parte derecha
    if(pNode.GetRightNode() != null)
    {
      ConnectRooms(pNode.GetRightNode());

      if(pNode.GetRoom() != null)
      {
        pNode.GetRoom().GetComponent<t_RoomCreator>().Connect();
        return;
      }
    }
    else
    {
      if(pNode.GetRoom() != null)
      {
        pNode.GetRoom().GetComponent<t_RoomCreator>().Connect();
        return;
      }
    }
  }

  // Limpia los objetos tiles del nivel
  private void EraseLevelObjects()
  {
    for(int i = 0; i < tileObjectList.Count; i++)
    {
      Destroy(tileObjectList[i]);
    }
    tileObjectList = new List<Object>();
  }

  // Crea las paredes y suelo del nivel
  private void DrawLevel()
  {
    int levelWidth = levelGrid.GetWidth();
    int levelHeight = levelGrid.GetHeight();

    //GameObject floor = Instantiate(floorTile);
    //floor.name = "Floor";
    //floor.tag = "Floor";
    //floor.transform.position = new Vector3(transform.position.x + levelWidth / 2, 1, transform.position.z + levelHeight / 2);
    //floor.transform.localScale = new Vector3(levelWidth, levelHeight, 1);
    //// Establecemos el tiling del material con respecto a sus dimensiones, un tile por unidad
    //floor.GetComponent<Renderer>().material.mainTextureScale = new Vector2(levelWidth, levelHeight);

    for(int i = 0; i < levelWidth; i++)
    {
      for(int j = 0; j < levelHeight; j++)
      {
        switch(levelGrid.GetTile(i, j))
        {
          case 1:
            tileObjectList.Add(Instantiate(floorTile, new Vector3(transform.position.x - (transform.localScale.x / 2) + i, transform.position.y + transform.localScale.y / 2 + 0.1f, transform.position.z - (transform.localScale.z / 2) + j), Quaternion.identity));
            break;
          case 2:
            tileObjectList.Add(Instantiate(wallTile, new Vector3(transform.position.x - (transform.localScale.x / 2) + i, transform.position.y + transform.localScale.y / 2, transform.position.z - (transform.localScale.z / 2) + j), Quaternion.identity));
            break;
        }

      }
    }
  }

  // Realiza un pase de automata celular para limpiar tiles sueltos o que sobresalen
  private void CleanLevelGeneration()
  {
    for(int k = 0; k < 5; k++)
    {
      for(int i = 0; i < levelGrid.GetWidth(); i++)
      {
        for(int j = 0; j < levelGrid.GetHeight(); j++)
        {
          RemoveSingles(i, j);
        }
      }
    }
  }

  // Automata celular para limpiar
  private void RemoveSingles(int x, int y)
  {
    int count = 0;

    if(x < levelGrid.GetWidth() - 1 && x > 1 && y > 1 && y < levelGrid.GetHeight() - 1)
    {
      if(levelGrid.GetTile(x + 1, y) == 1)
        count++;
      if(levelGrid.GetTile(x - 1, y) == 0)
        return;
      if(levelGrid.GetTile(x + 1, y) == 0)
        return;
      if(levelGrid.GetTile(x, y + 1) == 0)
        return;
      if(levelGrid.GetTile(x, y - 1) == 0)
        return;

      if(levelGrid.GetTile(x - 1, y) == 1)
        count++;
      if(levelGrid.GetTile(x, y + 1) == 1)
        count++;
      if(levelGrid.GetTile(x, y - 1) == 1)
        count++;
      if(levelGrid.GetTile(x - 1, y) == 1)
        count++;
      if(levelGrid.GetTile(x - 1, y - 1) == 1)
        count++;
      if(levelGrid.GetTile(x + 1, y - 1) == 1)
        count++;
      if(levelGrid.GetTile(x - 1, y + 1) == 1)
        count++;
      if(levelGrid.GetTile(x + 1, y + 1) == 1)
        count++;

      if(count >= 5)
        levelGrid.SetTile(x, y, 1);
    }
  }
}
