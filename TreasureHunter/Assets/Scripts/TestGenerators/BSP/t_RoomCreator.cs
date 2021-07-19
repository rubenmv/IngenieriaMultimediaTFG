using UnityEngine;
using System.Collections;

public class t_RoomCreator : MonoBehaviour
{
  public GameObject digger;
  private GeneratorBSP generator;

  private int roomID;
  private t_BSPNode parentNode;
  private GameObject sibling;

  public void Setup()
  {
    generator = GameObject.Find("GeneratorBSP").GetComponent<GeneratorBSP>();
    transform.position = new Vector3((int)transform.position.x, (int)transform.position.y, (int)transform.position.z);
    transform.position = new Vector3(transform.position.x - (transform.localScale.x / 2), transform.position.y, transform.position.z - (transform.localScale.z / 2));
    for(int i = (int)transform.position.x; i < (int)transform.position.x + transform.localScale.x; i++)
    {
      for(int j = (int)transform.position.z; j < (int)transform.position.z + transform.localScale.z; j++)
      {
        generator.SetTile(i, j, 1);
      }
    }

    for(int i = 0; i < transform.localScale.x + 1; i++)
    {
      generator.SetTile((int)transform.position.x + i, (int)transform.position.z, 2);
      generator.SetTile((int)transform.position.x + i, (int)(transform.position.z + transform.localScale.z), 2);
    }

    for(int i = 0; i < transform.localScale.z + 1; i++)
    {
      generator.SetTile((int)transform.position.x, (int)transform.position.z + i, 2);
      generator.SetTile((int)(transform.position.x + transform.localScale.x), (int)transform.position.z + i, 2);
    }

  }

  public void SetID(int aID)
  {
    roomID = aID;
  }

  public int GetID()
  {
    return roomID;
  }

  public void SetParentNode(t_BSPNode aNode)
  {
    parentNode = aNode;
  }

  public void Connect()
  {
    GetSibiling();

    if(sibling != null)
    {

      Vector3 startPos = new Vector3();
      Vector3 endPos = new Vector3();

      if(sibling.transform.position.z + sibling.transform.localScale.z < transform.position.z)
      {
        startPos = ChooseDoorPoint(0);
        endPos = sibling.GetComponent<t_RoomCreator>().ChooseDoorPoint(2);
      }
      else if(sibling.transform.position.z > transform.position.z + transform.localScale.z)
      {
        startPos = ChooseDoorPoint(2);
        endPos = sibling.GetComponent<t_RoomCreator>().ChooseDoorPoint(1);
      }
      else if(sibling.transform.position.x + sibling.transform.localScale.x < transform.position.x)
      {
        startPos = ChooseDoorPoint(3);
        endPos = sibling.GetComponent<t_RoomCreator>().ChooseDoorPoint(1);
      }
      else if(sibling.transform.position.x > transform.position.x + transform.localScale.x)
      {
        startPos = ChooseDoorPoint(1);
        endPos = sibling.GetComponent<t_RoomCreator>().ChooseDoorPoint(3);
      }


      GameObject aDigger = (GameObject)Instantiate(digger, startPos, Quaternion.identity);
      aDigger.GetComponent<t_Digger>().Begin(endPos);


      parentNode = FindRoomlessParent(parentNode);

      if(parentNode != null)
      {

        int aC = Random.Range(0, 2);

        if(aC == 0)
        {
          parentNode.SetRoom(this.gameObject);
        }
        else
        {
          parentNode.SetRoom(sibling.gameObject);
        }

        sibling.GetComponent<t_RoomCreator>().SetParentNode(parentNode);
      }

    }

  }

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
      case 0:
        return new Vector3((int)(transform.position.x + Random.Range(1, transform.localScale.x - 2)), transform.position.y, (int)(transform.position.z));
      case 1:
        return new Vector3((int)(transform.position.x + transform.localScale.x), transform.position.y, (int)(transform.position.z + Random.Range(1, transform.localScale.z - 2)));
      case 2:
        return new Vector3((int)(transform.position.x + Random.Range(1, transform.localScale.x - 2)), transform.position.y, (int)(transform.position.z + transform.localScale.z));
      case 3:
        return new Vector3((int)(transform.position.x + 1), transform.position.y, (int)(transform.position.z + Random.Range(1, transform.localScale.z - 2)));
      default:
        return new Vector3(0, 0, 0);
    }
  }

  public t_BSPNode GetParent()
  {
    return parentNode;
  }

  public t_BSPNode FindRoomlessParent(t_BSPNode aNode)
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
