using UnityEngine;
using System.Collections;

public class t_BSPNode
{
  GameObject cube;
  t_BSPNode parentNode;
  t_BSPNode leftNode;
  t_BSPNode rightNode;
  Color myColor;

  private bool isConnected;

  GameObject room;

  public t_BSPNode()
  {
    isConnected = false;
    myColor = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
  }

  public void SetLeftNode(t_BSPNode node)
  {
    leftNode = node;
  }

  public void SetRightNode(t_BSPNode node)
  {
    rightNode = node;
  }

  public void SetParentNode(t_BSPNode node)
  {
    parentNode = node;
  }

  public t_BSPNode GetLeftNode()
  {
    return leftNode;
  }

  public t_BSPNode GetRightNode()
  {
    return rightNode;
  }

  public t_BSPNode GetParentNode()
  {
    return parentNode;
  }

  // Divide, en el eje x, la seccion nodo
  void SplitX(GameObject section)
  {
    float roomSize = GeneratorBSP.ROOM_SIZE;
    if (section.transform.localScale.x < (roomSize * 2))
    {
      return;
    }

    float xSplit = Random.Range(roomSize, section.transform.localScale.x - roomSize);

    if (xSplit > roomSize)
    {
      GameObject cube0 = GameObject.CreatePrimitive(PrimitiveType.Cube);
      cube0.transform.localScale = new Vector3(xSplit, section.transform.localScale.y, section.transform.localScale.z);
      cube0.transform.position = new Vector3(
                section.transform.position.x - ((xSplit - section.transform.localScale.x) / 2),
                section.transform.position.y,
                section.transform.position.z);
      cube0.GetComponent<Renderer>().material.color = new Color(Random.Range(0.0f, 1.0f),
                                                                Random.Range(0.0f, 1.0f),
                                                                Random.Range(0.0f, 1.0f));
      cube0.GetComponent<Renderer>().material.shader = Shader.Find("Unlit/Color");
      cube0.tag = "GenSection";
      leftNode = new t_BSPNode();
      leftNode.SetCube(cube0);
      leftNode.SetParentNode(this);

      GameObject cube1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
      float split1 = section.transform.localScale.x - xSplit;
      cube1.transform.localScale = new Vector3(split1, section.transform.localScale.y, section.transform.localScale.z);
      cube1.transform.position = new Vector3(
                section.transform.position.x + ((split1 - section.transform.localScale.x) / 2),
                section.transform.position.y,
                section.transform.position.z);
      cube1.GetComponent<Renderer>().material.color = new Color(Random.Range(0.0f, 1.0f),
                                                                Random.Range(0.0f, 1.0f),
                                                                Random.Range(0.0f, 1.0f));
      cube1.GetComponent<Renderer>().material.shader = Shader.Find("Unlit/Color");
      cube1.tag = "GenSection";
      rightNode = new t_BSPNode();
      rightNode.SetCube(cube1);
      rightNode.SetParentNode(this);

      GameObject.DestroyImmediate(section);
    }
  }

  // Divide, en el eje z, la seccion nodo
  void SplitZ(GameObject section)
  {
    float roomSize = GeneratorBSP.ROOM_SIZE;
    if (section.transform.localScale.z < (roomSize * 2))
    {
      return;
    }
    float zSplit = Random.Range(roomSize, section.transform.localScale.z - roomSize);
    float zSplit1 = section.transform.localScale.z - zSplit;

    if (zSplit > roomSize)
    {

      GameObject cube0 = GameObject.CreatePrimitive(PrimitiveType.Cube);
      cube0.transform.localScale = new Vector3(section.transform.localScale.x, section.transform.localScale.y, zSplit);
      cube0.transform.position = new Vector3(
                section.transform.position.x,
                section.transform.position.y,
                section.transform.position.z - ((zSplit - section.transform.localScale.z) / 2));
      cube0.GetComponent<Renderer>().material.color = new Color(Random.Range(0.0f, 1.0f),
                                                                Random.Range(0.0f, 1.0f),
                                                                Random.Range(0.0f, 1.0f));
      cube0.GetComponent<Renderer>().material.shader = Shader.Find("Unlit/Color");
      cube0.tag = "GenSection";
      leftNode = new t_BSPNode();
      leftNode.SetCube(cube0);
      leftNode.SetParentNode(this);

      GameObject cube1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
      cube1.transform.localScale = new Vector3(section.transform.localScale.x, section.transform.localScale.y, zSplit1);
      cube1.transform.position = new Vector3(
                section.transform.position.x,
                section.transform.position.y,
                section.transform.position.z + ((zSplit1 - section.transform.localScale.z) / 2));
      cube1.GetComponent<Renderer>().material.color = new Color(Random.Range(0.0f, 1.0f),
                                                                Random.Range(0.0f, 1.0f),
                                                                Random.Range(0.0f, 1.0f));
      cube1.GetComponent<Renderer>().material.shader = Shader.Find("Unlit/Color");
      cube1.tag = "GenSection";
      rightNode = new t_BSPNode();
      rightNode.SetCube(cube1);
      rightNode.SetParentNode(this);

      GameObject.DestroyImmediate(section);
    }
  }

  public void SetCube(GameObject cube)
  {
    this.cube = cube;
  }

  public GameObject GetCube()
  {
    return cube;
  }

  public void Cut()
  {
    float choice = Random.Range(0, 2);
    if (choice <= 0.5)
    {
      SplitX(cube);
    }
    else
    {
      SplitZ(cube);
    }
  }

  public Color GetColor()
  {
    return myColor;
  }

  public void SetRoom(GameObject room)
  {
    this.room = room;
  }

  public GameObject GetRoom()
  {
    return room;
  }

  public void SetConnected()
  {
    isConnected = true;
  }

  public bool GetIsConnected()
  {
    return isConnected;
  }

}
