using UnityEngine;
using System.Collections;

public class t_WallTile : MonoBehaviour
{
  public Vector2i coordinates;

  public t_WallTile(int x, int z)
  {
    coordinates.x = x;
    coordinates.z = z;
  }
}
