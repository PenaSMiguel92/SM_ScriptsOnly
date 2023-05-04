using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TrapType { Hole, ElectricalBox, RedDoor, BlueDoor, YellowDoor, GreenDoor, CoinDoor }
public interface ITrapState
{
    public bool TrapEnabled { get; set; }
    public TrapType Type { get; }
    public DeathType DeathType { get; }

}
public class trap_state : MonoBehaviour, ITrapState
{
    [SerializeField] private Sprite[] _sprites;
    [SerializeField] private TrapType _type;
    [SerializeField] private DeathType _deathType;
    private bool _trapEnabled;
    public bool TrapEnabled {get { return _trapEnabled; } set { _trapEnabled = value; } }
    public TrapType Type {get { return _type; } }
    public DeathType DeathType {get { return _deathType; } }

    private int _trapEnabledInt;
    void Update()
    {
        _trapEnabledInt = !_trapEnabled ? 1 : 0;
        gameObject.GetComponent<SpriteRenderer>().sprite = _sprites[_trapEnabledInt];
    }
}
