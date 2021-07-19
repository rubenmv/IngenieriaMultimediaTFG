using UnityEngine;
using System.Collections;

// Plano rejilla sobre el que realizar las subdivisiones
// para las mazmorras de tipo BSP
public class t_Grid
{
  private int gridWidth;
  private int gridHeight;

  private int[,] grid;

  public t_Grid(int width, int height)
  {
    gridWidth = width;
    gridHeight = height;

    grid = new int[gridWidth, gridHeight];

    for(int i = 0; i < gridWidth; i++)
    {
      for(int j = 0; j < gridHeight; j++)
      {
        grid[i, j] = 0;
      }
    }
  }

  public void SetTile(int x, int y, int value)
  {
    grid[x, y] = value;
  }

  public int GetTile(int x, int y)
  {
    return grid[x, y];
  }

  public int GetWidth()
  {
    return gridWidth;
  }

  public int GetHeight()
  {
    return gridHeight;
  }
}
