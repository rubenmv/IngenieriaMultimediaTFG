using UnityEngine;

[System.Serializable]
public struct Vector2i
{

  public int x, z;

  public Vector2i(int x, int z)
  {
    this.x = x;
    this.z = z;
  }

  public Vector3 ToVector3()
  {
    return new Vector3(x, 0f, z);
  }

  public static Vector2i operator +(Vector2i a, Vector2i b)
  {
    a.x += b.x;
    a.z += b.z;
    return a;
  }

  public static Vector2i ToVector2i(Vector2 vec2)
  {
    return new Vector2i((int)vec2.x, (int)vec2.y);
  }

  public static Vector2i ToVector2i(Vector3 vec3)
  {
    return new Vector2i((int)vec3.x, (int)vec3.z);
  }
}