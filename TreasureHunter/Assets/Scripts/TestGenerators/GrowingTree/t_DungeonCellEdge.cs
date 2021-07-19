using UnityEngine;

public abstract class t_DungeonCellEdge : MonoBehaviour
{
  public t_DungeonCell cell, otherCell;

  public t_Direction direction;

  public void Initialize(t_DungeonCell cell, t_DungeonCell otherCell, t_Direction direction)
  {
    this.cell = cell;
    this.otherCell = otherCell;
    this.direction = direction;
    cell.SetEdge(direction, this);
    transform.parent = cell.transform;
    transform.localPosition = Vector3.zero;
    transform.localRotation = direction.ToRotation();
  }
}