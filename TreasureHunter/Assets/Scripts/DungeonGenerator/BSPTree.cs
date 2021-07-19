using UnityEngine;
using System.Collections;

public class BSPTree
{
  // Nodo raiz
  private BSPNode root;

  public BSPNode Root
  {
    get
    {
      return this.root;
    }
  }

  public BSPTree(BSPNode rootNode)
  {
    root = rootNode;
  }
}
