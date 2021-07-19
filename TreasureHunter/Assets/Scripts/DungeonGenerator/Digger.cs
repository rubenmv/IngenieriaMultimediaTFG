using UnityEngine;
using System.Collections;

public class Digger : MonoBehaviour
{
  public int roomId; // El pasillo tambien es una habitacion

  private DungeonGenerator generator;
  private Vector2i digger, end;


  //Color color;

  private void Awake()
  {

    generator = GameObject.FindObjectOfType<DungeonGenerator>();
  }

  // Crear suelo alrededor de la posicion actual y la rodea con paredes
  private void UpdateTile()
  {
    //generator.SetTile(digger.x, digger.z, 100);
    //GameManager.Instance.objectManager.CreateCube(digger.ToVector3(), color);

    // Inicializa los tiles del pasillo
    generator.SetTile(digger.x, digger.z, (int)TileType.FLOOR);
    generator.SetTile(digger.x + 1, digger.z, (int)TileType.FLOOR);
    generator.SetTile(digger.x - 1, digger.z, (int)TileType.FLOOR);
    generator.SetTile(digger.x, digger.z + 1, (int)TileType.FLOOR);
    generator.SetTile(digger.x, digger.z - 1, (int)TileType.FLOOR);
    // Rodea los tiles con pared
    SurroundTilesWithWall(digger.x + 1, digger.z);
    SurroundTilesWithWall(digger.x - 1, digger.z);
    SurroundTilesWithWall(digger.x, digger.z + 1);
    SurroundTilesWithWall(digger.x, digger.z - 1);
  }

  // Avanza en x,z para actualiza cada tile del pasillo a cavar
  public void Dig(Vector2i startPos, Vector2i targetPos)
  {
    digger = startPos;
    end = targetPos;
    //color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));

    //Vector3 offset = new Vector3(0.5f, 0.5f, 0.5f);
    //Vector3 p = digger.ToVector3() + offset;
    //GameManager.Instance.objectManager.CreateCube(p, Color.green);
    //p = end.ToVector3() + offset;
    //GameManager.Instance.objectManager.CreateCube(p, Color.red);

    while(digger.x != end.x)
    {
      if(digger.x < end.x)
      {
        digger.x++;
      }
      else
      {
        digger.x--;
      }
      // El tile final se queda vacio
      if(digger.x != end.x)
      {
        UpdateTile();
      }
    }

    while(digger.z != end.z)
    {
      if(digger.z < end.z)
      {
        digger.z++;
      }
      else
      {
        digger.z--;
      }
      UpdateTile();
    }

    Destroy(this.gameObject);
  }
  // Crea un tile de tipo pared si este esta vacio
  public void SurroundTilesWithWall(int x, int y)
  {
    Grid grid = generator.GetGrid();

    //int c = 0;
    ////// Arriba, izquierda, derecha, abajo (casillas impares)
    //for (int i = x - 1; i <= x + 1; i++) {
    //  for (int j = y - 1; j <= y + 1; j++) {
    //    if (c % 2 != 0 && grid.GetTile(i, j) == 0) {
    //      grid.SetTile(i, j, 2);
    //    }
    //    c++;
    //  }
    //}

    if(grid.GetTile(x + 1, y) == (int)TileType.EMPTY) // Derecha
    {
      generator.SetTile(x + 1, y, roomId);
    }
    if(grid.GetTile(x - 1, y) == (int)TileType.EMPTY) // Izquierda
    {
      generator.SetTile(x - 1, y, roomId);
    }
    if(grid.GetTile(x, y + 1) == (int)TileType.EMPTY) // Superior
    {
      generator.SetTile(x, y + 1, roomId);
    }
    if(grid.GetTile(x, y - 1) == (int)TileType.EMPTY) // Inferior
    {
      generator.SetTile(x, y - 1, roomId);
    }
  }
}
