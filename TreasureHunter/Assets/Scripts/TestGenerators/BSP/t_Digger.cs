using UnityEngine;
using System.Collections;

public class t_Digger : MonoBehaviour
{

  private Vector3 targetPos;
  private GeneratorBSP generator;

  public void Begin(Vector3 _targetPos)
  {
    targetPos = _targetPos;

    Dig();
  }

  private void UpdateTile()
  {
    generator = GameObject.Find("GeneratorBSP").GetComponent<GeneratorBSP>();
    generator.SetTile((int)transform.position.x, (int)transform.position.z, 1);
    generator.SetTile((int)transform.position.x + 1, (int)transform.position.z, 1);
    generator.SetTile((int)transform.position.x - 1, (int)transform.position.z, 1);
    generator.SetTile((int)transform.position.x, (int)transform.position.z + 1, 1);
    generator.SetTile((int)transform.position.x, (int)transform.position.z - 1, 1);

    SurroundTilesWithWall((int)transform.position.x + 1, (int)transform.position.z);
    SurroundTilesWithWall((int)transform.position.x - 1, (int)transform.position.z);
    SurroundTilesWithWall((int)transform.position.x, (int)transform.position.z + 1);
    SurroundTilesWithWall((int)transform.position.x, (int)transform.position.z - 1);

  }

  public void Dig()
  {

    while(transform.position.x != targetPos.x)
    {

      if(transform.position.x < targetPos.x)
      {
        transform.position = new Vector3(transform.position.x + 1, transform.position.y, transform.position.z);
      }
      else
      {
        transform.position = new Vector3(transform.position.x - 1, transform.position.y, transform.position.z);
      }

      UpdateTile();
    }

    while(transform.position.z != targetPos.z)
    {
      if(transform.position.z < targetPos.z)
      {
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + 1);
      }
      else
      {
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - 1);
      }

      UpdateTile();
    }

    DestroyImmediate(this);
  }

  public void SurroundTilesWithWall(int _x, int _y)
  {
    if(generator.GetGrid().GetTile(_x + 1, _y) == 0)
    {
      generator.SetTile(_x + 1, _y, 2);
    }

    if(generator.GetGrid().GetTile(_x - 1, _y) == 0)
    {
      generator.SetTile(_x - 1, _y, 2);
    }

    if(generator.GetGrid().GetTile(_x, _y + 1) == 0)
    {
      generator.SetTile(_x, _y + 1, 2);
    }

    if(generator.GetGrid().GetTile(_x, _y - 1) == 0)
    {
      generator.SetTile(_x, _y - 1, 2);
    }
  }
}
