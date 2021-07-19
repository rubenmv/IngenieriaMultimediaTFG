using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

// Tipos de tiles y numero identificador base de habitaciones
public enum TileType
{
  // Vacio
  EMPTY = 0,
  // Suelo
  FLOOR = 1,
  // Items
  COIN = 10,
  CHEST,
  POTION_HEALTH,
  // Trampas
  TRAP_SPIKES_FLOOR,
  TRAP_SPIKES_WALL,
  TRAP_ARROWS,
  TRAP_ROCK,
  ENEMY_CRAB,
  ENEMY_GOBLIN,
  EXIT,
  // Habitaciones y pasillos
  ROOM_ID = 50 // A partir de este id seran habitaciones y pasillos
}

// Direcciones para trampas y otros objetos. 
// Es importante que se mantenga este orden ya que:
public enum Direction
{
  NORTH = 0,
  EAST,
  SOUTH,
  WEST,
  NONE
}

public static class Tools
{
  public static bool IsBetween(this int item, int lower, int upper, bool inclusive)
  {
    return inclusive
        ? lower <= item && item <= upper
        : lower < item && item < upper;
  }
}

public class DungeonGenerator : MonoBehaviour
{
  private struct EnemyInfo
  {
    public TileType type;
    public bool patrol;
    public Vector2i position;
    public Vector2i patrolPoint;
  }

  private List<EnemyInfo> enemyList;

  // Dimensiones
  public int DUNGEON_WIDTH;
  public int DUNGEON_HEIGHT;
  // Dimensiones minimas del espacio donde se va a crear la habitacion
  public static float NODE_MIN_SIZE = 12f;
  public GameObject BaseRoom; // Objeto base para generar habitacion
  private ObjectManager objectManager;
  private Coroutine routine;

  // Margen dentro del espacio del nodo. Hay que tener en cuenta que sera el doble
  private const float NODE_MARGIN = 4f;
  // Unidad base de los tiles/celdas del mapa. Corresponde a unidades del grid de Unity
  private const float TILE_UNIT = 1f;
  // ROOM
  // Las dimensiones minimas de la habitacion seran la mitad de NODE_MIN_SIZE
  // Las dimensiones maximas seran NODE_MIN_SIZE - ROOM_MARGIN


  // Plano/rejilla sobre el que realizar las subdivisiones
  [HideInInspector]
  public Grid
    levelGrid; // Habitaciones
  [HideInInspector]
  public Grid
    objectsGrid; // Items y trampas
  [HideInInspector]
  public int
    nextId;
  [HideInInspector]
  public int
    lastRoomId = 0; // Ultimo id perteneciente a una habitacion (no pasillo)
  [HideInInspector]
  public BSPTree
    bspTree;
  private List<Room> rooms = new List<Room>(); // Lista de habitaciones (no pasillos) para acceso rapido
  private int level; // Numero de nivel para escoger las particiones bsp

  // ESTADISTICAS DE OBJETOS, ENEMIGOS...
  // Probabilidades
  private int probCoin = 80;
  private int probPotion = 20;
  private int probChest = 20;
  // Cantidad
  private int numEnemies = 0;
  private int numPotions = 0;
  private int numChests = 0;
  // Dificultad de la mazmorra
  private int difficulty = 0;

  private Vector3 playerStartPosition;

  // Objetos padres para agrupar los componentes de la mazmorra en el editor de Unity
  private GameObject parentScenery;
  private GameObject parentRooms;
  private GameObject parentItems;
  private GameObject parentTraps;
  private GameObject parentEnemies;

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
  public void GenerateDungeon(int width, int height)
  {
    // Crea los parents
    parentScenery = new GameObject();
    parentScenery.name = "Scenery";
    parentRooms = new GameObject();
    parentRooms.name = "Rooms";
    parentItems = new GameObject();
    parentItems.name = "Items";
    parentTraps = new GameObject();
    parentTraps.name = "Traps";
    parentEnemies = new GameObject();
    parentEnemies.name = "Enemies";

    objectManager = GameManager.Instance.objectManager;
    level = GameManager.Instance.GetLevel();
    DUNGEON_WIDTH = width;
    DUNGEON_HEIGHT = height;
    levelGrid = new Grid(DUNGEON_WIDTH, DUNGEON_HEIGHT);
    objectsGrid = new Grid(DUNGEON_WIDTH, DUNGEON_HEIGHT);
    nextId = 0;
    lastRoomId = nextId;
    enemyList = new List<EnemyInfo>();
    // Inicializamos los tiles a vacio
    for(int i = 0; i < levelGrid.GetWidth(); i++)
    {
      for(int j = 0; j < levelGrid.GetHeight(); j++)
      {
        levelGrid.SetTile(i, j, (int)TileType.EMPTY);
      }
    }
    // Creamos el nodo raiz con las dimensiones totales de la mazmorra
    // lo posicionamos en el centro del mapa
    BSPNode rootNode = new BSPNode();
    rootNode.size = new Vector2(DUNGEON_WIDTH, DUNGEON_HEIGHT);
    rootNode.position = new Vector2(transform.position.x + DUNGEON_WIDTH / 2, transform.position.z + DUNGEON_HEIGHT / 2);
    rootNode.branch = Branch.ROOT;
    rootNode.level = 0;
    bspTree = new BSPTree(rootNode);

    // Crea los nodos del arbol subdividiendo el plano inicial
    // finalmente lanza la creacion de las habitaciones, pasillos y paredes de la mazmorra
    routine = StartCoroutine(CreateDungeon());
  }

  IEnumerator CreateDungeon()
  {
    // Si utilizamos una semilla fija la generacion es siempre igual
    // para un nivel concreto
    //Random.seed = 3;

    int step = 0;
    int maxSteps = 10 + level;
    while(step < maxSteps)
    {
      Split(bspTree.Root);
      step++;
      yield return null;
    }

    // Dibujamos el resultado final del particionado
    //DrawLeafs(bspTree.Root);

    // Crea las habitaciones
    CreateRooms(bspTree.Root);
    // Conecta las habitaciones con pasillos
    ConnectRooms(bspTree.Root);

    // Limpieza con automata celular
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

    // Objetos, trampas y enemigos
    SetStartEndPoints();
    PopulateDungeon(bspTree.Root);
    PopulatePassages();
    DrawLevel();

    // Situa al jugador
    GameObject.FindGameObjectWithTag("Player").transform.position = playerStartPosition;

    difficulty = numEnemies * 10 - numPotions * 5;
    // Info de debug
    string info = "Dungeon dimensions: " + DUNGEON_WIDTH + "x" + DUNGEON_HEIGHT;
    info += "\nLevel: " + level;
    info += "\nDifficulty: " + difficulty;
    GameObject.Find("DebugTools").GetComponent<DebugInfo>().AddInfo(info);
  }

  private void SetStartEndPoints()
  {
    Room entrance = null;
    Room exit = null;
//    for(int i = 0; i < rooms.Count; i++)
//    {
//      Debug.Log(i + " -- " + rooms[i].GetNode());
//      if(rooms[i].GetNode() == null)
//      {
//        Debug.Log("NULL");
//      }
//    }
    // Entrance
    for(int i = 0; i < rooms.Count; i++)
    {
      if(rooms[i].GetNode() == null)
      {
        continue;
      }
      if(rooms[i].GetNode().branch == Branch.LEFT)
      {
        entrance = rooms[i];
        entrance.role = RoomRole.ENTRANCE;
        playerStartPosition = entrance.transform.position;
        playerStartPosition.y += 2f;
        break;
      }
    }
    // Exit
    if(entrance != null)
    {
      for(int i = 0; i < rooms.Count; i++)
      {
        if(rooms[i].GetNode() != null && rooms[i].GetNode().branch == Branch.RIGHT)
        {
          if(exit == null)
          {
            exit = rooms[i];
          }
          else if(Vector3.Distance(entrance.position.ToVector3(), rooms[i].position.ToVector3()) >
            Vector3.Distance(entrance.position.ToVector3(), exit.position.ToVector3()))
          {// Nos quedamos con la habitacion mas alejada de la entrada
            exit = rooms[i];
          }
        }
      }
      exit.role = RoomRole.EXIT;
    }
    else
    {
      Debug.LogWarning("Couldn't stablish entrance/exit");
    }
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

  private Vector2i GetRandomCoordinateInRoom(Room room)
  {
    bool found = false;
    int tries = 0;
    int rx = room.position.x;
    int rz = room.position.z;
    int depth = 2;
    while(!found && tries < 20)
    {
      // No queremos posiciones en pared o adyacentes a pared
      rx = Random.Range(room.position.x + depth, room.position.x + room.size.x - depth);
      rz = Random.Range(room.position.z + depth, room.position.z + room.size.z - depth);

      if(levelGrid.GetTile(rx, rz) == (int)TileType.FLOOR &&
        objectsGrid.GetTile(rx, rz) == (int)TileType.EMPTY)
      {
        found = true;
      }
      tries++;
    }
    Vector2i position = new Vector2i(rx, rz);
    if(!found)
    {
      position = new Vector2i(-1, -1);
    }
    return position;
  }

  // Situa al jugador en la entrada de la mazmorra
  private void PlacePlayer(Room room)
  {
    Vector3 position = room.transform.position;
    position.y += 2f;
    GameObject.FindGameObjectWithTag("Player").transform.position = position;
  }

  // Coloca el objeto de salida de la mazmorra
  private void PlaceExit(Room room)
  {
    int roomId = room.GetId();
    Vector2i coord = Vector2i.ToVector2i(room.transform.position); // Centro habitacion
    Vector2i searchLimits = new Vector2i(room.size.x / 2, room.size.z / 2);

    // Posicion en pared +z
    if(levelGrid.GetTile(coord.x, coord.z + searchLimits.z) == roomId)
    {
      objectsGrid.SetTile(coord.x, coord.z + searchLimits.z, (int)TileType.EXIT);
    }// Posicion en pared -z
    else if(levelGrid.GetTile(coord.x, coord.z - searchLimits.z) == roomId)
    {
      objectsGrid.SetTile(coord.x, coord.z - searchLimits.z, (int)TileType.EXIT);
    }// Posicion en pared +x
    else if(levelGrid.GetTile(coord.x + searchLimits.x, coord.z) == roomId)
    {
      objectsGrid.SetTile(coord.x + searchLimits.x, coord.z, (int)TileType.EXIT);
    }// Posicion en pared -x
    else if(levelGrid.GetTile(coord.x - searchLimits.x, coord.z) == roomId)
    {
      objectsGrid.SetTile(coord.x - searchLimits.x, coord.z, (int)TileType.EXIT);
    }
    else
    {
      objectsGrid.SetTile(coord.x, coord.z, (int)TileType.EXIT);
    }
  }

  // Situa ememigos dentro de la habitacion
  private void PlaceItems(Room room)
  {
    // Actualiza el grid de objetos
    Vector2i coord = GetRandomCoordinateInRoom(room);
    if(coord.x == -1)
    {
      return;
    }

    int r = Random.Range(0, 100);
    // Crea un item segun su probabilidad

    bool isBigRoom = false;
    float bigRoomSize = NODE_MIN_SIZE - NODE_MARGIN / 2;
    if((room.size.x > bigRoomSize || room.size.z > bigRoomSize))
    {
      isBigRoom = true;
    }

    // Las monedas siempre pueden aparecer
    if(r >= 0 && r < probCoin)
    { // Coin
      int coins = Random.Range(2, 5);
      if(isBigRoom)
      {
        coins = Random.Range(4, 7);
      }
      // Pueden aparecer mas de una moneda
      for(int i = 0; i < coins; i++)
      { // Probabilidad igual
        if(coord.x == -1)
        {
          continue;
        }
        objectsGrid.SetTile(coord.x, coord.z, (int)TileType.COIN);
        coord = GetRandomCoordinateInRoom(room);
      }
    }

    if(coord.x == -1)
    {
      return;
    }
    // Tambien puede aparecer una pocion o un cofre
    r = Random.Range(0, 100);
    // Pociones solo en la segunda mitad de la mazmorra
    if(room.GetNode() != null && room.GetNode().branch == Branch.RIGHT && r >= 0 && r < probPotion)
    { // Potion
      objectsGrid.SetTile(coord.x, coord.z, (int)TileType.POTION_HEALTH);
      numPotions++;
      if(probPotion > 0)
      {
        probPotion -= 10;
      }
    }
    else if(r >= probPotion && r < probPotion + probChest)
    { // Chest
      objectsGrid.SetTile(coord.x, coord.z, (int)TileType.CHEST);
      numChests++;
      if(probChest > 0)
      {
        probChest -= 5;
      }
    }
  }

  private void SorroundChestWithTraps(Vector2i chestPosition, TileType selectedTrap)
  {

    if(selectedTrap == TileType.TRAP_SPIKES_FLOOR)
    {// Rodea el cofre con suelo de pinchos
      for(int x = chestPosition.x - 1; x <= chestPosition.x + 1; x++)
      {
        for(int z = chestPosition.z - 1; z <= chestPosition.z + 1; z++)
        {
          if(!(x == chestPosition.x && z == chestPosition.z))
          { // No a si mismo
            if(levelGrid.GetTile(x, z) == (int)TileType.FLOOR)
            { // El espacio del escenario es suelo
              objectsGrid.SetTile(x, z, (int)TileType.TRAP_SPIKES_FLOOR);
            }
          }
        }
      }
    }
    else
    { // Coloca flechas en las paredes cerca del cofre
      int steps = 0;
      bool isTrapPlaced = false;
      Vector2i[] vDirections = { new Vector2i(1, 0), new Vector2i(-1, 0),
                                  new Vector2i(0, 1), new Vector2i(0, -1) };
      // Vamos a poner un offset sobre la posicion del cofre para que los
      // dispensadores de flechas disparen hacia los lados de este
      Vector2i currentPosition = chestPosition;
      for(int i = 0; i < vDirections.Length; i++)
      {
        steps = 0;
        currentPosition = chestPosition + new Vector2i(-1, -1);
        while(steps < 5)
        {
          currentPosition += vDirections[i];
          steps++;
          if(levelGrid.GetTile(currentPosition.x, currentPosition.z) >= (int)TileType.ROOM_ID)
          {
            isTrapPlaced = true;
            objectsGrid.SetTile(currentPosition.x, currentPosition.z, (int)TileType.TRAP_ARROWS);
            break;
          }
        }
      }
      // Como salvaguarda, si no se ha podido colocar ninguna trampa
      // rodeamos el cofre con pinchos
      if(!isTrapPlaced)
      {
        SorroundChestWithTraps(chestPosition, TileType.TRAP_SPIKES_FLOOR);
      }
    }
  }

  // Coloca trampas en la habitacion
  private void PlaceTraps(Room room)
  {
    Vector2i start = room.position;
    Vector2i end = room.position + room.size;
    // Rodear los cofres con pinchos
    // Aunque la geometria de la habitacion haya sido modificada seguimos contando con el mismo
    // area para recorrer su parte del grid
    for(int i = start.x; i < end.x; i++)
    {
      for(int j = start.z; j < end.z; j++)
      {
        switch(objectsGrid.GetTile(i, j))
        {
          case (int)TileType.CHEST:
            int randomTrap = Random.Range(0, 2);
            Vector2i chestPosition = new Vector2i(i, j);
            TileType trap = TileType.TRAP_SPIKES_FLOOR;
            if(randomTrap == 1)
            {
              trap = TileType.TRAP_ARROWS;
            }
            SorroundChestWithTraps(chestPosition, trap);

            break;
          case (int)TileType.COIN:
            // Si la moneda esta cerca de una pared, colocamos una trampa en ese tile de pared
            Vector2i xLimits = new Vector2i(i - 2, i + 2);
            Vector2i zLimits = new Vector2i(j - 2, j + 2);
            for(int x = xLimits.x; x <= xLimits.z; x++)
            {
              for(int z = zLimits.x; z <= zLimits.z; z++)
              {
                if(!(x == xLimits.x && z == zLimits.x) && !(x == xLimits.x && z == zLimits.z) &&
                  !(x == xLimits.z && z == zLimits.x) && !(x == xLimits.z && z == zLimits.z))
                {
                  if(levelGrid.GetTile(x, z) >= (int)TileType.ROOM_ID)
                  { // Evitamos dos paredes pinchos en una misma esquina
                    if(objectsGrid.GetTile(x - 1, z - 1) != (int)TileType.TRAP_SPIKES_WALL &&
                      objectsGrid.GetTile(x - 1, z + 1) != (int)TileType.TRAP_SPIKES_WALL &&
                      objectsGrid.GetTile(x + 1, z - 1) != (int)TileType.TRAP_SPIKES_WALL &&
                      objectsGrid.GetTile(x + 1, z + 1) != (int)TileType.TRAP_SPIKES_WALL)
                    {
                      objectsGrid.SetTile(x, z, (int)TileType.TRAP_SPIKES_WALL);
                    }
                  }
                }
              }
            }
            break;
        }
      }
    }
  }

  // Agrega trampas a pasillos
  private void PopulatePassages()
  {
    // Pasillos visitados que ya no se van a comprobar
    List<int> visitedPassages = new List<int>();
    // Tiles del pasillo actual
    List<Vector2i> passageTiles = new List<Vector2i>();
    int cellCount = 0;
    for(int i = 0; i < DUNGEON_WIDTH; i++)
    {
      for(int j = 0; j < DUNGEON_HEIGHT; j++)
      {
        int tile = levelGrid.GetTile(i, j);
        int currentPassage = tile;
        // Encuentra tile de pasillo no visitado
        if(tile > lastRoomId && visitedPassages.FindIndex(item => item == tile) == -1)
        {
          // Recorrido horizontal
          for(int x = j; tile == currentPassage && x < DUNGEON_HEIGHT; x++)
          {
            passageTiles.Add(new Vector2i(i, x));
            cellCount++;
            tile = levelGrid.GetTile(i, x);
          }
          // ROCK
          if(cellCount >= 10)
          { // Se puede poner una roca rodante
            objectsGrid.SetTile(i + 2, j, (int)TileType.TRAP_ROCK);
          }
          // ARROWS. Volvemos a recorrer el pasillo poniendo flechas
          else if(cellCount >= 5)
          {
            // Tiles intermedios del pasillo
            for(int c = 1; c < passageTiles.Count - 1; c++)
            {
              Vector2i position = passageTiles[c];
              int placement = Random.Range(0, 2);
              if(placement == 0 && levelGrid.GetTile(position.x + 4, position.z) > lastRoomId)
              {
                position.x += 4;
              }
              objectsGrid.SetTile(position.x, position.z, (int)TileType.TRAP_ARROWS);
            }
          }
          cellCount = 0;
          visitedPassages.Add(currentPassage);
          passageTiles.Clear();
        }
      }
    }
  }

  // Coloca enemigos en la habitacion
  private void PlaceEnemies(Room room)
  {
    bool isBigRoom = false;
    float bigRoomSize = NODE_MIN_SIZE - NODE_MARGIN / 2;
    if((room.size.x > bigRoomSize || room.size.z > bigRoomSize))
    {
      isBigRoom = true;
    }

    // Buscamos cofres o monedas en la habitacion
    Vector2i start = room.position;
    Vector2i end = room.position + room.size;
    int countCoins = 0;
    Vector2i chestPosition = new Vector2i();
    for(int i = start.x; i < end.x; i++)
    {
      for(int j = start.z; j < end.z; j++)
      {
        switch(objectsGrid.GetTile(i, j))
        {
          case (int)TileType.CHEST:
            chestPosition = new Vector2i(i, j);
            break;
          case (int)TileType.COIN:
            countCoins++;
            break;
        }
      }
    }

    if(chestPosition.x != 0) // Cofre protegido
    {
      int r = Random.Range(0, 3);// 33.3% de probabilidad
      if(r < 2)
      {
        // Crea goblin patrullando cofre
        EnemyInfo info = new EnemyInfo();
        info.type = TileType.ENEMY_GOBLIN;
        info.patrol = true;
        info.position = new Vector2i(chestPosition.x + 1, chestPosition.z + 1);
        info.patrolPoint = chestPosition;
        enemyList.Add(info);
        numEnemies++;
      }
    }

    if(level >= 4 && isBigRoom) // Habitacion grande
    {
      int enemiesToCreate = Random.Range(2, 5);
      for(int i = 0; i < enemiesToCreate; i++)
      {
        Vector2i coord = GetRandomCoordinateInRoom(room);
        if(coord.x == -1)
        {
          continue;
        }
        int r = Random.Range(0, 3);
        EnemyInfo info = new EnemyInfo();
        switch(r)
        {
          case 0: // Crab
          case 1:
            objectsGrid.SetTile(coord.x, coord.z, (int)TileType.ENEMY_CRAB);
            info.type = TileType.ENEMY_CRAB;
            info.patrol = true;
            info.position = new Vector2i(coord.x, coord.z);
            info.patrolPoint = coord;
            enemyList.Add(info);
            break;
          case 2: // Goblin
            objectsGrid.SetTile(coord.x, coord.z, (int)TileType.ENEMY_GOBLIN);
            info.type = TileType.ENEMY_GOBLIN;
            info.patrol = true;
            info.position = new Vector2i(coord.x, coord.z);
            info.patrolPoint = coord;
            enemyList.Add(info);
            break;
        }
        numEnemies++;
      }
    }
    else if(countCoins >= 4 && Random.Range(0, 3) < 2)
    {
      bool found = true;
      Vector2i coord = GetRandomCoordinateInRoom(room);
      if(coord.x == -1)
      {
        found = false;
      }
      if(found)
      {
        // Crea goblin patrullando monedas
        objectsGrid.SetTile(coord.x, coord.z, (int)TileType.ENEMY_GOBLIN);
        EnemyInfo info = new EnemyInfo();
        info.type = TileType.ENEMY_GOBLIN;
        info.patrol = true;
        info.position = new Vector2i(coord.x, coord.z);
        info.patrolPoint = coord;
        enemyList.Add(info);
        numEnemies++;
      }
    }
  }

  private void PopulateDungeon(BSPNode node)
  {
    // Si hay nodo izquierdo nos movemos un nivel
    if(node.GetLeftNode() != null)
    {
      PopulateDungeon(node.GetLeftNode());
    }
    else
    {// Hoja con habitacion
      Room room = node.GetRoom().GetComponent<Room>();

      if(room.role != RoomRole.ENTRANCE)
      {
        PlaceItems(room);
        PlaceTraps(room);
        PlaceEnemies(room);
        if(room.role == RoomRole.EXIT)
        {
          PlaceExit(room);
        }
      }

      return;
    }
    if(node.GetRightNode() != null)
    {
      PopulateDungeon(node.GetRightNode());
    }
  }

  //split the tree
  public void Split(BSPNode node)
  {
    // Si hay nodo izquierdo nos movemos un nivel
    if(node.GetLeftNode() != null)
    {
      Split(node.GetLeftNode());
    }
    else
    {
      // Cuando no hay nodos hijos, intenta crearlos
      node.Cut();
      return;
    }
    if(node.GetRightNode() != null)
    {
      Split(node.GetRightNode());
    }
  }

  public Grid GetGrid()
  {
    return levelGrid;
  }

  public void SetTile(int x, int y, int value)
  {
    levelGrid.SetTile(x, y, value);
  }

  // Agrega una habitacion dentro de los limites del espacio del nodo
  private Room AddRoom(BSPNode node)
  {
    // Las dimensiones minimas de la habitacion seran la mitad de NODE_MIN_SIZE
    // Las dimensiones maximas seran NODE_MIN_SIZE - ROOM_MARGIN
    Vector3 roomPosition = new Vector3(node.position.x, 0f, node.position.y);
    GameObject aRoom = (GameObject)Instantiate(BaseRoom, roomPosition, Quaternion.identity);
    aRoom.transform.localScale = new Vector3((int)(Random.Range(NODE_MIN_SIZE / 2, node.size.x - NODE_MARGIN)),
                                             aRoom.transform.localScale.y,
                                             (int)(Random.Range(NODE_MIN_SIZE / 2, node.size.y - NODE_MARGIN)));
    aRoom.transform.parent = parentRooms.transform;
    // Configuracion de la habitacion
    Room roomScript = aRoom.GetComponent<Room>();
    // Id unico
    roomScript.SetID((int)TileType.ROOM_ID + nextId);
    lastRoomId = (int)TileType.ROOM_ID + nextId;
    nextId++;
    roomScript.role = RoomRole.DEFAULT;
    aRoom.name = node.GetBranchName() + "-" + node.level.ToString();
    // Escribe en el grid la estructura de la habitacion
    levelGrid = roomScript.Setup(levelGrid);
    roomScript.SetNode(node);
    node.SetRoom(aRoom);

    return roomScript;
  }

  public void DrawNode(BSPNode node)
  {
    GameObject cube1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
    cube1.transform.localScale = new Vector3(node.size.x, 0.1f, node.size.y);
    cube1.transform.position = new Vector3(node.position.x, 0f, node.position.y);
    cube1.GetComponent<Renderer>().material.color = new Color(Random.Range(0.0f, 1.0f),
                                                              Random.Range(0.0f, 1.0f),
                                                              Random.Range(0.0f, 1.0f));
    cube1.GetComponent<Renderer>().material.shader = Shader.Find("Unlit/Color");
  }

  // Dibuja en la escena las ultimas subdivisiones en la creacion de la mazmorra
  private void DrawLeafs(BSPNode node)
  {
    if(node.GetLeftNode() != null)
    {
      DrawLeafs(node.GetLeftNode());
    }
    else
    {
      DrawNode(node);
      return;
    }

    if(node.GetRightNode() != null)
    {
      DrawLeafs(node.GetRightNode());
    }
  }

  private void CreateRooms(BSPNode node)
  {
    if(node == null)
    {
      return;
    }
    if(node.GetLeftNode() != null)
    {
      CreateRooms(node.GetLeftNode());
    }
    if(node.GetLeftNode() == null && node.GetLeftNode() == null)
    {
      rooms.Add(AddRoom(node));
    }
    if(node.GetRightNode() != null)
    {
      CreateRooms(node.GetRightNode());
    }
  }

  // Crea los pasillos para conectar las habitaciones
  // Recorre recursivamente el arbol
  private void ConnectRooms(BSPNode node)
  {
    // Recorrido por los nodos de la parte izquierda
    if(node.GetLeftNode() != null)
    {
      ConnectRooms(node.GetLeftNode());

      if(node.GetRoom() != null)
      {
        node.GetRoom().GetComponent<Room>().Connect();
        return;
      }
    }
    else
    {
      if(node.GetRoom() != null)
      {
        node.GetRoom().GetComponent<Room>().Connect();
        return;
      }
    }
    // Recorrido por los nodos de la parte derecha
    if(node.GetRightNode() != null)
    {
      ConnectRooms(node.GetRightNode());

      if(node.GetRoom() != null)
      {
        node.GetRoom().GetComponent<Room>().Connect();
        return;
      }
    }
    else
    {
      if(node.GetRoom() != null)
      {
        node.GetRoom().GetComponent<Room>().Connect();
        return;
      }
    }
  }

  private Vector2i GetAdyacentWallDirection(int x, int z, int depth)
  {
    Vector2i direction = new Vector2i(0, 0);
    if(levelGrid.GetTile(x - depth, z) != 0)
    {
      direction.x = -1;
    }
    else if(levelGrid.GetTile(x + depth, z) != 0)
    {
      direction.x = 1;
    }
    else if(levelGrid.GetTile(x, z - depth) != 0)
    {
      direction.z = -1;
    }
    else if(levelGrid.GetTile(x, z + depth) != 0)
    {
      direction.z = 1;
    }
    return direction;
  }

  private GameObject CreateWall(List<Vector2i> coordinates, char axis)
  {
    if(coordinates.Count <= 0)
    {
      return null;
    }
    GameObject wall = null;
    float halfUnit = TILE_UNIT / 2;
    float gap = 0.02f;
    Vector3 position = new Vector3(coordinates[0].x + halfUnit, 0f, coordinates[0].z + halfUnit); // En el centro de la celda
    if(coordinates.Count == 1)
    {// Celdas solitarias
      if(axis == 'x')
      {
        wall = objectManager.Create(ObjectName.TileWall, new Vector3(position.x, 0f, position.z));
        Vector3 wallScale = wall.transform.localScale;
        wallScale.x += gap;
        wallScale.z += gap;
        wall.transform.localScale = wallScale;
        wall.GetComponent<TextureTiling>().ReTiling();
      }
    }
    else if(coordinates.Count > 1)
    { // Agrupar tiles en un pared
      Vector3 midPoint = coordinates[coordinates.Count / 2].ToVector3();
      int wallWidth = coordinates.Count;
      wall = objectManager.Create(ObjectName.TileWall, new Vector3(midPoint.x, 0f, midPoint.z));
      // Al escalar tenemo en cuenta que vamos a hacer tiling de la textura
      // por lo que en vez de escalar en z (columnas, c) que es por donde estamos uniendo tiles
      // vamos a escalar en x y luego rotar, así el retiling de la textura se aplica sobre la coordenada correcta
      Vector3 wallScale = wall.transform.localScale;
      wallScale.x = wallWidth + gap;
      wallScale.z += gap;
      wall.transform.localScale = wallScale;
      wall.GetComponent<TextureTiling>().ReTiling(); // Establece el tiling de las texturas segun la escala
      // Ajustes para centrar la pared dentro de las celdas
      if(axis == 'x')
      {
        wall.transform.Translate(0f, 0f, halfUnit);
        if(wallWidth % 2 != 0)
        {
          wall.transform.Translate(halfUnit, 0f, 0f);
        }
      }
      if(axis == 'z')
      {
        wall.transform.Translate(halfUnit, 0f, 0f);
        if(wallWidth % 2 != 0)
        {
          wall.transform.Translate(0f, 0f, halfUnit);
        }
        wall.transform.Rotate(Vector3.up, 90f);
      }
    }

    return wall;
  }

  private void DrawLevel()
  {
    GameObject tempObject;
    // SUELO
    // Instanciamos el prefab del suelo y lo escalamos
    Vector3 position = new Vector3(transform.position.x + DUNGEON_WIDTH / 2, 0f, transform.position.z + DUNGEON_HEIGHT / 2);
    tempObject = objectManager.Create(ObjectName.TileFloor, position);
    tempObject.transform.localScale = new Vector3(DUNGEON_WIDTH, DUNGEON_HEIGHT, 1);
    // Establecemos el tiling del material con respecto a sus dimensiones, un tile por unidad
    tempObject.GetComponent<Renderer>().material.mainTextureScale = new Vector2(DUNGEON_WIDTH, DUNGEON_HEIGHT);
    tempObject.transform.parent = parentScenery.transform;
    // TECHO
    // Instanciamos el prefab del techo y lo escalamos
    position = new Vector3(transform.position.x + DUNGEON_WIDTH / 2, 5f, transform.position.z + DUNGEON_HEIGHT / 2);
    tempObject = objectManager.Create(ObjectName.TileCeiling, position);
    tempObject.transform.localScale = new Vector3(DUNGEON_WIDTH, 1f, DUNGEON_HEIGHT);
    // Establecemos el tiling del material con respecto a sus dimensiones, un tile por unidad
    //tempObject.GetComponent<Renderer>().material.mainTextureScale = new Vector2(DUNGEON_WIDTH, DUNGEON_HEIGHT);
    tempObject.transform.parent = parentScenery.transform;
    tempObject.GetComponent<TextureTiling>().ReTiling();

    // PAREDES
    List<Vector2i> wallCoordinates = new List<Vector2i>();

    Grid tempGrid = new Grid(DUNGEON_WIDTH, DUNGEON_HEIGHT);
    tempGrid.SetGrid(levelGrid.GetGrid());

    // Crear el escenario, uniendo paredes
    // Paredes Eje z
    for(int r = 0; r < DUNGEON_WIDTH; r++)
    { //x
      for(int c = 0; c < DUNGEON_HEIGHT; c++)
      { //z
        if(tempGrid.GetTile(r, c) >= (int)TileType.ROOM_ID)
        {
          wallCoordinates.Add(new Vector2i(r, c));
        }
        else
        {
          tempObject = CreateWall(wallCoordinates, 'z');
          if(tempObject != null)
          {
            tempObject.transform.parent = parentScenery.transform;
          }
          if(wallCoordinates.Count > 1)
          {
            for(int i = 0; i < wallCoordinates.Count; i++)
            { // Borra la pared del mapa temporal
              tempGrid.SetTile(wallCoordinates[i].x, wallCoordinates[i].z, 0);
            }
          }
          wallCoordinates = new List<Vector2i>();
        }
      }
    }

    // Paredes Eje x y celdas solitarias restantes
    for(int c = 0; c < DUNGEON_HEIGHT; c++)
    { //z
      for(int r = 0; r < DUNGEON_WIDTH; r++)
      { //x
        if(tempGrid.GetTile(r, c) >= (int)TileType.ROOM_ID)
        {
          wallCoordinates.Add(new Vector2i(r, c));
        }
        else
        {
          tempObject = CreateWall(wallCoordinates, 'x');
          if(tempObject != null)
          {
            tempObject.transform.parent = parentScenery.transform;
          }
          for(int i = 0; i < wallCoordinates.Count; i++)
          { // Borra la pared del mapa temporal
            tempGrid.SetTile(wallCoordinates[i].x, wallCoordinates[i].z, 0);
          }
          wallCoordinates = new List<Vector2i>();
        }
      }
    }

    float startDelay = 0f; // Para trampas

    // OBJETOS
    for(int r = 0; r < DUNGEON_WIDTH; r++)
    { //x
      for(int c = 0; c < DUNGEON_HEIGHT; c++)
      { //z
        position = new Vector3(r + TILE_UNIT / 2, 0f, c + TILE_UNIT / 2); // En el centro de la celda
        int cell = objectsGrid.GetTile(r, c);
        // Objetos
        switch(cell)
        {
          case (int)TileType.COIN:
            tempObject = objectManager.Create(ObjectName.Coin, position);
            tempObject.transform.parent = parentItems.transform;
            break;
          case (int)TileType.CHEST:
            // Los cofres cerca de una pared se giran sobre esta
            tempObject = objectManager.Create(ObjectName.Chest, position);
            Vector3 direction = GetAdyacentWallDirection(r, c, 1).ToVector3();
            direction.x *= -1f;
            tempObject.transform.LookAt(position + direction);
            tempObject.transform.parent = parentItems.transform;
            break;
          case (int)TileType.POTION_HEALTH:
            tempObject = objectManager.Create(ObjectName.PotionHealth, position);
            tempObject.transform.parent = parentItems.transform;
            break;
          case (int)TileType.TRAP_SPIKES_WALL:
          case (int)TileType.TRAP_ARROWS:
            position = new Vector3(r, TILE_UNIT / 1.5f, c);
            Direction orientation = CalculateTrapDirection(new Vector2i(r, c));
            if(orientation != Direction.NONE)
            {
              Vector3 rotation = new Vector3(90f, 0f, 0f);
              if(orientation == Direction.NORTH)
              {
                position.x += TILE_UNIT / 2;
                position.z += TILE_UNIT;
                rotation = new Vector3(0f, 0f, 0f);
              }
              else if(orientation == Direction.EAST)
              {
                position.x += TILE_UNIT;
                position.z += TILE_UNIT / 2;
                rotation = new Vector3(0f, 90f, 0f);
              }
              else if(orientation == Direction.SOUTH)
              {
                position.x += TILE_UNIT / 2;
                rotation = new Vector3(0f, 180f, 0f);
              }
              else if(orientation == Direction.WEST)
              {
                position.z += TILE_UNIT / 2;
                rotation = new Vector3(0f, 270f, 0f);
              }
            
              ObjectName objectName = (cell == (int)TileType.TRAP_SPIKES_WALL) ? ObjectName.TrapSpikeWall : ObjectName.TrapArrows;

              tempObject = objectManager.Create(objectName, position);
              tempObject.transform.eulerAngles = rotation;
              tempObject.transform.parent = parentTraps.transform;


              if(objectName == ObjectName.TrapSpikeWall)
              {
                tempObject.GetComponentInChildren<TrapAnimation>().startDelay = startDelay;
                startDelay += 0.5f;
                if(startDelay > 2f)
                {
                  startDelay = 0f;
                }
              }
            }
            else
            {
              // Restauramos el tile para que no aparezca en las metricas
              objectsGrid.SetTile(r, c, (int)TileType.EMPTY);
            }

            break;
          case (int)TileType.TRAP_SPIKES_FLOOR:
            tempObject = objectManager.Create(ObjectName.TrapSpikeFloor, position);
            tempObject.transform.parent = parentTraps.transform;

            tempObject.GetComponentInChildren<TrapAnimation>().startDelay = startDelay;
            startDelay += 0.5f;
            if(startDelay > 2f)
            {
              startDelay = 0f;
            }
            break;
          case (int)TileType.TRAP_ROCK:
            tempObject = objectManager.Create(ObjectName.TrapRock, position);
            tempObject.transform.parent = parentTraps.transform;
            break;
          case (int)TileType.EXIT:
            objectManager.Create(ObjectName.Portal, position);
            break;
        }
      }
    }

    // ENEMIGOS
    for(int i = 0; i < enemyList.Count; i++)
    {
//      case (int)TileType.ENEMY_CRAB:
//      position.y += TILE_UNIT;
//      tempObject = objectManager.Create(ObjectName.EnemyCrab, position);
//      tempObject.GetComponent<Enemy>().SetPatrolCenter(position);
//      tempObject.transform.parent = parentEnemies.transform;
//      break;
//      case (int)TileType.ENEMY_GOBLIN:
//      position.y += TILE_UNIT;
//      tempObject = objectManager.Create(ObjectName.EnemyGobling, position);
//      tempObject.GetComponent<Enemy>().SetPatrolCenter(position);
//      tempObject.transform.parent = parentEnemies.transform;
//      break;
      ObjectName objName = enemyList[i].type == TileType.ENEMY_GOBLIN ? ObjectName.EnemyGobling : ObjectName.EnemyCrab;
      position = enemyList[i].position.ToVector3();
      position.y += TILE_UNIT;
      tempObject = objectManager.Create(objName, position);
      tempObject.GetComponent<Enemy>().SetPatrolCenter(enemyList[i].patrolPoint.ToVector3());
      tempObject.transform.parent = parentEnemies.transform;

    }

  }

  // Mira los alrededores de la posicion de la trampa para orientarla hacia el lado correcto
  private Direction CalculateTrapDirection(Vector2i pos)
  {
    List<Direction> candidates = new List<Direction>();
    Direction orientation = Direction.NONE;

    //// Arriba, izquierda, derecha, abajo (casillas impares)
    int c = 0;
    for(int i = pos.x - 1; i <= pos.x + 1; i++)
    {
      for(int j = pos.z - 1; j <= pos.z + 1; j++)
      {
        if(c % 2 != 0 && levelGrid.GetTile(i, j) == (int)TileType.FLOOR)
        {
          if(c == 1)
          {
            candidates.Add(Direction.WEST);
          }
          else if(c == 3)
          {
            candidates.Add(Direction.SOUTH);
          }
          else if(c == 5)
          {
            candidates.Add(Direction.NORTH);
          }
          else if(c == 7)
          {
            candidates.Add(Direction.EAST);
          }
        }
        c++;
      }
    }

    if(candidates.Count == 1)
    {
      orientation = candidates[0];
    }
    else if(candidates.Count > 1)
    { // Escoge una direccion aleatoriamente
      orientation = candidates[Random.Range(0, candidates.Count)];
    }

    return orientation;
  }

  // Automata celular para limpiar
  private void RemoveSingles(int x, int y)
  {
    int count = 0;

    if(x < levelGrid.GetWidth() - 1 && x > 1 && y > 1 && y < levelGrid.GetHeight() - 1)
    {
      if(levelGrid.GetTile(x + 1, y) == (int)TileType.FLOOR)
        count++;
      if(levelGrid.GetTile(x - 1, y) == (int)TileType.EMPTY)
        return;
      if(levelGrid.GetTile(x + 1, y) == (int)TileType.EMPTY)
        return;
      if(levelGrid.GetTile(x, y + 1) == (int)TileType.EMPTY)
        return;
      if(levelGrid.GetTile(x, y - 1) == (int)TileType.EMPTY)
        return;

      if(levelGrid.GetTile(x - 1, y) == (int)TileType.FLOOR)
        count++;
      if(levelGrid.GetTile(x, y + 1) == (int)TileType.FLOOR)
        count++;
      if(levelGrid.GetTile(x, y - 1) == (int)TileType.FLOOR)
        count++;
      if(levelGrid.GetTile(x - 1, y) == (int)TileType.FLOOR)
        count++;
      if(levelGrid.GetTile(x - 1, y - 1) == (int)TileType.FLOOR)
        count++;
      if(levelGrid.GetTile(x + 1, y - 1) == (int)TileType.FLOOR)
        count++;
      if(levelGrid.GetTile(x - 1, y + 1) == (int)TileType.FLOOR)
        count++;
      if(levelGrid.GetTile(x + 1, y + 1) == (int)TileType.FLOOR)
        count++;

      if(count >= 5)
        levelGrid.SetTile(x, y, (int)TileType.FLOOR);
    }
  }

  // Vuelca a fichero el resultado de la generación
  public void SaveToFile()
  {
    string fileName = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
    fileName = "bsp_" + fileName;
    string content = "Dungeon Generator (BSP)";
    //content += "\nSeed: " + seed;
    content += "\nLevel: " + level;
    content += "\nDificulty: " + difficulty;
    content += "\nDimensions: " + DUNGEON_WIDTH + "x" + DUNGEON_HEIGHT;
    content += "\nRoom min size: " + NODE_MIN_SIZE / 2;
    content += "\n2 caracteres = 1 celda";
    content += "\n\n";

    string temp;
    int tile;
    // Layout de las habitaciones y pasillos
    for(int x = 0; x < DUNGEON_WIDTH; x++)
    { // alto = filas
      for(int z = 0; z < DUNGEON_HEIGHT; z++)
      { // ancho = columnas
        if(levelGrid.GetTile(x, z) == 0)
        {
          content += "--";
        }
        else
        {
          tile = levelGrid.GetTile(x, z);
          if(tile < 10)
          {
            temp = "0" + tile.ToString();
          }
          else
          {
            temp = tile.ToString();
          }
          content += temp;
        }
      }
      content += "\n";
    }
    content += "\n\n";


    // Habitaciones + objetos/trampas
    for(int x = 0; x < DUNGEON_WIDTH; x++)
    { // alto = filas
      for(int z = 0; z < DUNGEON_HEIGHT; z++)
      { // ancho = columnas
        if(objectsGrid.GetTile(x, z) == 0)
        {
          if(levelGrid.GetTile(x, z) == 0)
          {
            content += "##";
          }
          else
          {
            tile = levelGrid.GetTile(x, z);
            if(tile < 10)
            {
              temp = "0" + tile.ToString();
            }
            else
            {
              temp = tile.ToString();
            }
            content += temp;
          }
        }
        else
        {
          tile = objectsGrid.GetTile(x, z);
          if(tile < 10)
          {
            temp = "0" + tile.ToString();
          }
          else
          {
            temp = tile.ToString();
          }
          content += temp;
        }
      }
      content += "\n";
    }

    // Escribe a fichero
    fileName = DebugTools.instance.METRICS_PATH + "BSP/" + fileName + @".txt";
    DebugTools.instance.WriteToFile(fileName, content);
  }
}
