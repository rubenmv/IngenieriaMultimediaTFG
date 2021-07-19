using UnityEngine;
using System.Collections;

public class t_BSPTree
{
  // Nodo raiz
  private t_BSPNode root;

  public t_BSPTree(GameObject rootArea)
  {
    root = new t_BSPNode();
    root.SetCube(rootArea);
  }

  public t_BSPNode Root
  {
    get
    {
      return this.root;
    }
  }
}
