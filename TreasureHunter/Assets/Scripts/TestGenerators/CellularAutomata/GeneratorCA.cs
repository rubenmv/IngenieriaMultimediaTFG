using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Generacion basada en el metodo de automata celular por Jim Badcock
// para crear cuevas de manera procedural. Se basa en la regla 4-5:
// Un se establece como pared si ya lo era y al menos 4 de sus vecinos lo son
// o aun no era pared y al menos 5 de sus vecinos lo son
// http://www.roguebasin.com/index.php?title=Cellular_Automata_Method_for_Generating_Random_Cave-Like_Levels
public class GeneratorCA : MonoBehaviour
{
  public t_WallTile wallTilePrefab;
  private int[,] grid;
  private int passes; // Pases/iteraciones que realizara el algoritmo
  // Dimensiones en tiles
  private int width, height;
  // Porcentaje de probabilidad de aparicion de pared en el tablero inicial
  private int wallProbability = 40;
  private int seed;
  List<t_WallTile> wallTileList = new List<t_WallTile>();

  public bool doNextStep = false;

  public void Cleanup()
  {
    foreach (t_WallTile item in wallTileList)
    {
      Destroy(item.gameObject);
    }
  }

  private int RandomByProbability(int probability)
  {
    // Si la probabilidad es mayor que el random obtenido,
    // devuelve confirmacion
    int randomN = Random.Range(1, 101); // 1 - 100
    if (probability >= randomN)
    {
      return 1;
    }
    return 0;
  }

  public void Generate(int width, int height, int passes, int wallProbability, int seed)
  {
    this.width = width;
    this.height = height;
    this.passes = passes;
    this.wallProbability = wallProbability;
    if (seed >= 0)
    {
      this.seed = seed;
      Random.seed = this.seed;
    }
    else
    {
      this.seed = Random.seed;
    }

    // Creamos una nueva rejilla y la rellenamos con ruido aleatorio
    // segun la probabilidad de que aparezca pared
    wallTileList = new List<t_WallTile>();
    grid = new int[width, height];

    StartCoroutine(CreateDungeon());
  }

  private IEnumerator CreateDungeon()
  {
    // Crear el mapa inicial de ruido usando la probabilidad de paredes
    FillRandom();

    // Dibujar el mapa de ruido
    DrawDungeon();

    while (!doNextStep)
    {
      yield return null;
    }
    doNextStep = false;

    int r, c;
    for (int t = 0; t < passes; t++)
    {
      r = 0;
      while (r < height)
      {
        c = 0;
        while (c < width)
        {
          // Obtenemos numero de paredes adyacentes a esta posicion
          int numWalls = GetAdyacentWalls(c, r);

          if (grid[c, r] == 1) // Ya hay pared
          {
            // Si el numero de paredes adyacentes es >= 4, sigue siendo pared
            // en caso contrario se convierte en espacio
            if (numWalls <= 2)
            {
              grid[c, r] = 0; // Vacio
              // Cuando un elemento se convierte en vacio, eliminamos el objeto asociado
              for (int i = 0; i < wallTileList.Count; i++)
              {
                if (wallTileList[i].coordinates.x == c && wallTileList[i].coordinates.z == r)
                {
                  Destroy(wallTileList[i].gameObject);
                  wallTileList.RemoveAt(i);
                  break;
                }
              }
            }
          }
          else // Es vacio, si tiene 5 o mas paredes adyacentes, se convierte en pared
          {
            if (numWalls >= 5)
            {
              grid[c, r] = 1; // Pared
              t_WallTile tile = Instantiate(wallTilePrefab, new Vector3((float)c, 0f, (float)r), Quaternion.identity) as t_WallTile;
              tile.coordinates = new Vector2i(c, r);
              wallTileList.Add(tile);
            }
          }
          c++;
        }
        r++;
      }
    }

    GameObject.Find("TestGenerators").GetComponent<TestGenerators>().SetStepButton(false);

    // Volcamos al historial de mapas generados
    WriteToFile();
  }

  // Vuelca a fichero el resultado de la generación
  private void WriteToFile()
  {
    string fileName = System.DateTime.Now.ToString("ca_yyyyMMdd_HHmmss");
    string content = "Cellular Automata";
    content += "\nSeed: " + seed;
    content += "\nDimensions: " + width + "x" + height;
    content += "\nWall probability: " + wallProbability + "%";
    content += "\nPasses: " + passes;
    content += "\n\n";

    for (int r = 0; r < height; r++) // alto = filas
    {
      for (int c = 0; c < width; c++) // ancho = columnas
      {
        if (grid[c, r] == 1)
        {
          content += "#";
        }
        else
        {
          content += " ";
        }
      }
      content += "\n";
    }

    // Escribe a fichero
    fileName = DebugTools.instance.METRICS_PATH + "Cellular Automata/" + fileName + @".txt";
    DebugTools.instance.WriteToFile(fileName, content);
  }

  // Rellena la rejilla inicial de manera aleatoria
  private void FillRandom()
  {
    int gridMiddle = 0;
    for (int r = 0; r < height; r++) // c = column, r = row
    {
      for (int c = 0; c < width; c++)
      {
        // Siempre que estemos en el borde creamos pared
        if (c == 0 || c == width - 1 ||
          r == 0 || r == height - 1)
        {
          grid[c, r] = 1;
        }
        // En cualquier otro caso establecemos la pared segun su probabilidad aleatoria
        else
        {
          gridMiddle = (height / 2);
          if (r == gridMiddle)
          {
            grid[c, r] = 0; // Espacio vacio
          }
          else
          {
            grid[c, r] = RandomByProbability(wallProbability);
          }
        }
      }
    }
  }

  // Obtiene el numero de paredes adyacentes
  private int GetAdyacentWalls(int x, int y)
  {
    int startX = x - 1;
    int startY = y - 1;
    int endX = x + 1;
    int endY = y + 1;

    int iX = startX;
    int iY = startY;

    int count = 0;

    // Recorremos las filas de tiles adyacentes y contamos paredes
    for (iY = startY; iY <= endY; iY++)
    {
      for (iX = startX; iX <= endX; iX++)
      {
        if (!(iX == x && iY == y) && IsWall(iX, iY))
        {
          count++;
        }
      }
    }
    return count;
  }

  // Indica si el tile es una pared
  private bool IsWall(int x, int y)
  {
    // Fuera del mapa, cuenta como pared
    if (x < 0 || x >= width || y < 0 || y >= height)
    {
      return true;
    }
    else if (grid[x, y] == 1) // Dentro de mapa pero es pared
    {
      return true;
    }
    // En cualquier otro caso no es pared
    return false;
  }

  // Crea los objetos/tiles para rellenar las parede de la mazmorra
  private void DrawDungeon()
  {
    for (int r = 0; r < height; r++) // c = column, r = row
    {
      for (int c = 0; c < width; c++)
      {
        if (grid[c, r] == 1)
        {
          t_WallTile tile = Instantiate(wallTilePrefab, new Vector3((float)c, 0f, (float)r), Quaternion.identity) as t_WallTile;
          tile.coordinates = new Vector2i(c, r);
          wallTileList.Add(tile);
        }
      }
    }
  }
}
