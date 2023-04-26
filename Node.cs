using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    private bool crossable;
    private Vector3 location;

    private Node parent;
    
    public Node(bool _crossable, Vector3 _location)
    {
        crossable = _crossable;
        location = _location;
    }

}
