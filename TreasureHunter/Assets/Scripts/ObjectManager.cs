using UnityEngine;
using System.Collections;

public enum ObjectName
{
  Player = 0,
  TileFloor,
  TileWall,
  TrapArrows,
  TrapSpikeFloor,
  TrapSpikeWall,
  TrapRock,
  Portal,
  Coin,
  PotionHealth,
  Chest,
  EnemyCrab,
  EnemyGobling,
  TileCeiling
}

public class ObjectManager : MonoBehaviour
{

  public GameObject[] ObjectPrefabs;
  public GameObject Create(ObjectName name, Vector3 position)
  {
    GameObject objectPrefab = ObjectPrefabs[(int)name];
    GameObject objectInstance;

    // Las paredes se colocan 
    if(name == ObjectName.TileWall)
    {
      position.y += objectPrefab.transform.localScale.y / 2 - 0.1f;
    }

    objectInstance = (GameObject)Instantiate(objectPrefab, position, objectPrefab.transform.rotation);

    return objectInstance;
  }

  public GameObject CreateCube(Vector3 position, Color color)
  {
    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
    cube.transform.position = position;
    cube.GetComponent<Renderer>().material.color = color;
    return cube;
  }
}
