using UnityEngine;

public class t_DungeonCell : MonoBehaviour
{

  public Vector2i coordinates;

  private t_DungeonCellEdge[] edges = new t_DungeonCellEdge[t_DungeonDirections.Count];

  private int initializedEdgeCount;

  public bool IsFullyInitialized
  {
    get
    {
      return initializedEdgeCount == t_DungeonDirections.Count;
    }
  }

  public t_Direction RandomUninitializedDirection
  {
    get
    {
      int skips = Random.Range(0, t_DungeonDirections.Count - initializedEdgeCount);
      for(int i = 0; i < t_DungeonDirections.Count; i++)
      {
        if(edges[i] == null)
        {
          if(skips == 0)
          {
            return (t_Direction)i;
          }
          skips -= 1;
        }
      }
      throw new System.InvalidOperationException("MazeCell has no uninitialized directions left.");
    }
  }

  public t_DungeonCellEdge GetEdge(t_Direction direction)
  {
    return edges[(int)direction];
  }

  public void SetEdge(t_Direction direction, t_DungeonCellEdge edge)
  {
    edges[(int)direction] = edge;
    initializedEdgeCount += 1;
  }
}