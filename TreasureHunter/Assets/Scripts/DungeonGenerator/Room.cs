using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Tipos de habitaciones
public enum RoomRole
{
  DEFAULT,
  ENTRANCE, // Habitacion inicial donde comienza el jugador
  EXIT,     // Habitacion final de la mazmorra
  PASSAGE   // Pasillo que une dos habitaciones
}

public class Room : MonoBehaviour
{
  public GameObject prefabDigger;
  public RoomRole role;
  // Posicion y dimensiones en enteros. Son unidades de grid.
  public Vector2i position;
  public Vector2i size;
  // Indica si se debe escoger aleatoriamente cuando se asigna una habitacion a un nodo
  // padre para realizar las conexiones mediante pasillos
  // Cuando es false se escogen la habitaciones por cercania al centro
  public bool randomConnection = false;
  public List<Vector2i> doorPoints; // Puntos de puertas hacia pasillos
  private int roomId;
  private BSPNode parentNode;
  private GameObject sibling;

  public void SetID(int aID)
  {
    roomId = aID;
  }

  public int GetId()
  {
    return roomId;
  }

  public void SetNode(BSPNode aNode)
  {
    if(aNode == null)
    {
      Debug.Log("Setting node to null in room " + GetId());
    }
    parentNode = aNode;
  }

  public BSPNode GetNode()
  {
    return parentNode;
  }

  // Establece la geometria de la habitacion sobre el grid
  public Grid Setup(Grid grid)
  {
    doorPoints = new List<Vector2i>();
    // Pasamos la posicion y escala a unidades de grid
    // La posicion se situa en la esquina superior izquierda, la primera casilla de la habitacion
    this.size = new Vector2i((int)transform.localScale.x, (int)transform.localScale.z);
    this.position = new Vector2i((int)(transform.position.x - (size.x / 2)), (int)(transform.position.z - (size.z / 2)));

    // Inicializa todos los tiles de la habitacion
    for(int i = position.x; i < position.x + size.x; i++)
    {
      for(int j = position.z; j < position.z + size.z; j++)
      {
        grid.SetTile(i, j, (int)TileType.FLOOR);
      }
    }

    // Paredes de la habitacion
    for(int i = 0; i <= size.x; i++)
    {
      grid.SetTile(position.x + i, position.z, roomId); // Arriba
      grid.SetTile(position.x + i, position.z + size.z, roomId); // Abajo
    }
    for(int i = 0; i <= size.z; i++)
    {
      grid.SetTile(position.x, position.z + i, roomId); // Izquierda
      grid.SetTile(position.x + size.x, position.z + i, roomId); // Derecha
    }

    return grid;
  }

  public void Connect()
  {
    DungeonGenerator generator = GameObject.FindObjectOfType<DungeonGenerator>();
    // Recoge el nodo hermano
    GetSibiling();
    if(sibling != null)
    {
      Room siblingRoom = sibling.GetComponent<Room>();

      Vector2i startPos = new Vector2i(this.position.x + this.size.x / 2, this.position.z + this.size.z / 2);
      Vector2i endPos = new Vector2i(siblingRoom.position.x + siblingRoom.size.x / 2, siblingRoom.position.z + siblingRoom.size.z / 2);

      if(startPos.x == endPos.x || startPos.z == endPos.z)
      {
        if(startPos.x != endPos.x)
        {
          startPos.z = Random.Range(startPos.z - this.size.z / 2, startPos.z + this.size.z / 2);
        }
        else if(startPos.z != endPos.z)
        {
          startPos.x = Random.Range(startPos.x - this.size.x / 2, startPos.x + this.size.x / 2);
        }
      }

      GameObject digger = (GameObject)Instantiate(prefabDigger, this.transform.position, Quaternion.identity);
      digger.GetComponent<Digger>().roomId = (int)TileType.ROOM_ID + generator.nextId;
      generator.nextId++;
      digger.GetComponent<Digger>().Dig(startPos, endPos);

      // Buscar un padre sin habitacion (normalmente el padre inmediato)
      parentNode = FindRoomlessParent(parentNode);
      if(parentNode != null)
      {
        randomConnection = Random.Range(0, 2) == 0 ? true : false;
        // La habitacion se escoge aleatoriamente entre los dos nodos hijos
        if(randomConnection)
        {
          if(Random.Range(0, 2) == 0)
          {
            parentNode.SetRoom(this.gameObject);
          }
          else
          {
            parentNode.SetRoom(sibling.gameObject);
          }
        }
        // La que este mas cerca del centro de la mazmorra
        // Esto es una manera de escoger siempre la habitacion que esta en la parte
        // mas interior y asi evitar que al conectarlas por un pasillo, este corte otra habitacion
        else
        {
          Vector2 center = new Vector2(GameManager.Instance.levelManager.dungeonGenerator.GetComponent<DungeonGenerator>().DUNGEON_WIDTH / 2,
                                       GameManager.Instance.levelManager.dungeonGenerator.GetComponent<DungeonGenerator>().DUNGEON_HEIGHT / 2);
          if(Vector2.Distance(center, this.parentNode.position) <= Vector2.Distance(center, siblingRoom.parentNode.position))
          {
            parentNode.SetRoom(this.gameObject);
          }
          else
          {
            parentNode.SetRoom(sibling.gameObject);
          }
        }
        siblingRoom.SetNode(parentNode);
      }
      Destroy(digger);
    }
  }

  // Obtiene el nodo hermano
  private void GetSibiling()
  {
    if(parentNode.GetParentNode() != null)
    {
      if(parentNode.GetParentNode().GetLeftNode() != parentNode)
      {
        sibling = parentNode.GetParentNode().GetLeftNode().GetRoom();
      }
      else
      {
        sibling = parentNode.GetParentNode().GetRightNode().GetRoom();
      }
    }
  }

  public Vector3 ChooseDoorPoint(int index)
  {
    switch(index)
    {
      case 0: // Sur, se crea puerta en X, z
        return new Vector3((int)(position.x + Random.Range(1, size.x - 2)), transform.position.y, (int)(position.z));
      case 1: // Este, se crea puerta en x + size.x, Z
        return new Vector3((int)(position.x + size.x), transform.position.y, (int)(position.z + Random.Range(1, size.z - 2)));
      case 2: // Norte, se crea puerta en X, z + size.z
        return new Vector3((int)(position.x + Random.Range(1, size.x - 2)), transform.position.y, (int)(position.z + size.z));
      case 3: // Oeste, se crea puerta en x, Z
        return new Vector3((int)(position.x + 1), transform.position.y, (int)(position.z + Random.Range(1, size.z - 2)));
      default:
        return new Vector3(0f, 0f, 0f);
    }
  }

  // Sube por los padres hasta encontrar uno que no tenga habitacion
  public BSPNode FindRoomlessParent(BSPNode aNode)
  {
    if(aNode != null)
    {
      if(aNode.GetRoom() == null)
      {
        return aNode;
      }
      else
      {
        return FindRoomlessParent(aNode.GetParentNode());
      }
    }

    return null;
  }
}
