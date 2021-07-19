using UnityEngine;
using System.Collections;

public enum Branch
{
  ROOT,
  LEFT,
  RIGHT
}

public class BSPNode
{
  private BSPNode parentNode;
  private BSPNode leftNode;
  private BSPNode rightNode;
  private Color myColor;

  private bool isConnected;

  GameObject room;

  public Vector2 position;
  public Vector2 size;

  public Branch branch; // Left o right. Indica la rama, en base a root, a la que pertenece
  public int level; // Nivel de profundidad dentro del arbol

  public BSPNode()
  {
    isConnected = false;
    myColor = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
  }

  public string GetBranchName()
  {
    string _name = "root";
    switch (branch)
    {
      case Branch.LEFT:
        _name = "left";
        break;
      case Branch.RIGHT:
        _name = "right";
        break;
    }

    return _name;
  }

  public void SetLeftNode(BSPNode node)
  {
    leftNode = node;
  }

  public void SetRightNode(BSPNode node)
  {
    rightNode = node;
  }

  public void SetParentNode(BSPNode node)
  {
    parentNode = node;
  }

  public BSPNode GetLeftNode()
  {
    return leftNode;
  }

  public BSPNode GetRightNode()
  {
    return rightNode;
  }

  public BSPNode GetParentNode()
  {
    return parentNode;
  }

  void SplitX()
  {
    float minSplitSize = DungeonGenerator.NODE_MIN_SIZE;
    // Al realizar el corte debemos tener en cuenta que al menos deben entrar dos habitaciones de tamaño minimo
    if (this.size.x < (minSplitSize * 2))
    {
      return;
    }

    float xSplit = Random.Range(minSplitSize, this.size.x - minSplitSize);
    float xSplit1 = this.size.x - xSplit;

    // Creamos el nodo izquierdo con las dimensiones del nuevo corte
    leftNode = new BSPNode();
    leftNode.size = new Vector2(xSplit, this.size.y);
    leftNode.position = new Vector2(this.position.x - ((xSplit - this.size.x) / 2), this.position.y);

    // Indicar la rama en base a root
    if (this.branch == Branch.ROOT)
    {
      leftNode.branch = Branch.LEFT;
    }
    else
    {
      leftNode.branch = this.branch;
    }
    leftNode.level = level + 1;

    leftNode.SetParentNode(this);

    // Creamos el nodo derecho con las dimensiones del nuevo corte
    rightNode = new BSPNode();
    // Espacio restante
    rightNode.size = new Vector2(xSplit1, this.size.y);
    rightNode.position = new Vector2(this.position.x + ((xSplit1 - this.size.x) / 2), this.position.y);

    // Indicar la rama en base a root
    if (this.branch == Branch.ROOT)
    {
      rightNode.branch = Branch.RIGHT;
    }
    else
    {
      rightNode.branch = this.branch;
    }
    rightNode.level = level + 1;

    rightNode.SetParentNode(this);
  }

  void SplitZ()
  {
    float minSplitSize = DungeonGenerator.NODE_MIN_SIZE;
    if (this.size.y < (minSplitSize * 2))
    {
      return;
    }

    float zSplit = Random.Range(minSplitSize, this.size.y - minSplitSize);
    float zSplit1 = this.size.y - zSplit;

    leftNode = new BSPNode();
    leftNode.size = new Vector2(this.size.x, zSplit);
    leftNode.position = new Vector2(this.position.x, this.position.y - ((zSplit - this.size.y) / 2));

    // Indicar la rama en base a root
    if (this.branch == Branch.ROOT)
    {
      leftNode.branch = Branch.LEFT;
    }
    else
    {
      leftNode.branch = this.branch;
    }
    leftNode.level = level + 1;
    leftNode.SetParentNode(this);

    rightNode = new BSPNode();
    rightNode.size = new Vector2(this.size.x, zSplit1);
    rightNode.position = new Vector2(this.position.x, this.position.y + ((zSplit1 - this.size.y) / 2));

    // Indicar la rama en base a root
    if (this.branch == Branch.ROOT)
    {
      rightNode.branch = Branch.RIGHT;
    }
    else
    {
      rightNode.branch = this.branch;
    }
    rightNode.level = level + 1;
    rightNode.SetParentNode(this);

  }

  public void Cut()
  {
    float choice = Random.Range(0, 2);
    if (choice <= 0.5)
    {
      SplitX();
    }
    else
    {
      SplitZ();
    }
  }

  public Color GetColor()
  {
    return myColor;
  }

  public void SetRoom(GameObject room)
  {
    this.room = room;
  }

  public GameObject GetRoom()
  {
    return room;
  }

  public void SetConnected()
  {
    isConnected = true;
  }

  public bool GetIsConnected()
  {
    return isConnected;
  }

}
