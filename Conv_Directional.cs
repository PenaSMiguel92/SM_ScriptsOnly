using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ConvDir {Up, Down, Left, Right}

public interface IConveyor
{
    public ConvDir Direction { get; }
}

public class Conv_Directional : MonoBehaviour, IConveyor
{
    [SerializeField] private ConvDir _direction;
    [SerializeField] private Sprite[] _sprites;
    public ConvDir Direction {get { return _direction; } }

    



}
