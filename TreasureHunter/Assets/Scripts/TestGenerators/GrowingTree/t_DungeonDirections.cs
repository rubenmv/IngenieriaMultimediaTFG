using UnityEngine;

public enum t_Direction
{
  North,
  East,
  South,
  West
}

public static class t_DungeonDirections
{
  public const int Count = 4;

  public static t_Direction RandomValue
  {
    get
    {
      return (t_Direction)Random.Range(0, Count);
    }
  }

  private static t_Direction[] opposites = {
		t_Direction.South,
		t_Direction.West,
		t_Direction.North,
		t_Direction.East
	};

  public static t_Direction GetOpposite(this t_Direction direction)
  {
    return opposites[(int)direction];
  }
	
  private static Vector2i[] vectors = {
		new Vector2i(0, 1),
		new Vector2i(1, 0),
		new Vector2i(0, -1),
		new Vector2i(-1, 0)
	};
	
  public static Vector2i ToIntVector2(this t_Direction direction)
  {
    return vectors[(int)direction];
  }

  private static Quaternion[] rotations = {
		Quaternion.identity,
		Quaternion.Euler(0f, 90f, 0f),
		Quaternion.Euler(0f, 180f, 0f),
		Quaternion.Euler(0f, 270f, 0f)
	};
	
  public static Quaternion ToRotation(this t_Direction direction)
  {
    return rotations[(int)direction];
  }
}