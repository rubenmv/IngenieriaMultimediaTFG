using UnityEngine;
using System.Collections;

// Plano rejilla sobre el que realizar las subdivisiones
// y donde se establecen los tipos distintos de tiles del nivel
public class Grid
{
  private int gridWidth;
  private int gridHeight;

  private int[,] grid;

  public int[,] GetGrid()
  {
    return grid;
  }
  public void SetGrid(int[,] grid)
  {
    for(int i = 0; i < gridWidth; i++)
    {
      for(int j = 0; j < gridHeight; j++)
      {
        this.grid[i, j] = grid[i, j];
      }
    }
  }

  public Grid(int width, int height)
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
    if(x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
    {
      grid[x, y] = value;
    }
    else
    {
      Debug.LogWarning("Grid.SetTile: Trying to set a value of a tile out of bounds (" + x + ", " + y + ")");
    }
  }

  public int GetTile(int x, int y)
  {
    if(x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
    {
      return grid[x, y];
    }
    else
    {
      Debug.LogWarning("Grid.GetTile: Trying to get a value of a tile out of bounds (" + x + ", " + y + ")");
    }
    return -1;
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
