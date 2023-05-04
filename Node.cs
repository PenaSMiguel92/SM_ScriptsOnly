using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INode
{
    public bool Crossable { get; }
    public Vector3 Location { get; set; }
    public Node Parent { get; set; }

}

public class Node : INode
{
    private bool _crossable;
    private Vector3 _location;
    private Node _parent;
    public bool Crossable { get { return _crossable; } }
    public Vector3 Location { get { return _location; } set { _location = value; } }
    public Node Parent { get { return _parent; } set { _parent = value; } }


    public Node(bool crossable, Vector3 location)
    {
        _crossable = crossable;
        _location = location;
    }

}
