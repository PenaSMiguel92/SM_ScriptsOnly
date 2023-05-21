using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum TrapType { Hole, ElectricalBox}
public enum TrapState {Loading, Idle, Waiting, Crossing, Set, PlayerStand} 
public interface ITrapState
{
    public DeathType DeathType { get; }
    public Tuple<DeathType, bool, bool> TestTrap(Vector3Int _moveDirection, GameObject _invokingObject);
    public bool TestPushable(GameObject _invokingObject);
    // public void StoreTile(TileBase _targetTile);

}
public class TrapHandle : MonoBehaviour, ITrapState, IStateChange<TrapType, TrapState>
{
    [SerializeField] private Sprite[] _sprites;
    [SerializeField] private TrapType _type;
    [SerializeField] private DeathType _deathType;

    private TrapState _state = TrapState.Loading;
    public TrapState State { get { return _state; } }
    public TrapType Type {get { return _type; } }
    public DeathType DeathType {get { return _deathType; } }

    private int _trapStateIndex = 0; //0 = active, 1 = disabled
    private Transform _currentTransform;
    private Vector3Int _gridPos;
    private GameObject _pushable;
    private SpriteRenderer _spriteRenderer;
    private GameControl _mainControl;
    private Player _mainPlayer;

    void Start()
    {
        _mainControl = GameControl.Main;
        _mainPlayer = Player.Main;
        _mainControl.onGameStart += OnGameStart;
        _mainControl.onTrapStateChange += StateChange;
        
    }

    void OnGameStart(object _sender, EventArgs _e)
    {
        _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        _currentTransform = gameObject.GetComponent<Transform>();
        _gridPos = _mainControl.GetGridPosition(_currentTransform.localPosition);
        _state = TrapState.Idle;
    }
    
    void StateChange(object _sender, ITrapStateChange _e)
    {
        _trapStateIndex = _e.State ? 1 : 0;
        if (_e.State)
        {
            _state = TrapState.Set;
        }
        else
        {
            _state = TrapState.Idle;
        }
    }
    public Tuple<DeathType, bool, bool> TestTrap(Vector3Int _moveDirection, GameObject _invokingObject)
    {
        bool _move = false;
        bool _kill = false;
        GameObject tmp_pushable;
        PushableHandle tmp_pushableHandle;
        Debug.Log(_state);
        switch(_state)
        {
            case TrapState.Crossing:
                _kill = _trapStateIndex == 0 ? true : false;
                switch(_invokingObject.tag)
                {
                    case "enemy":
                        _move = false;
                        break;
                    case "player":
                        tmp_pushable = _mainControl.GetInstantiatedObject(_gridPos, TilemapUse.Moveables);
                        tmp_pushableHandle = tmp_pushable.GetComponent<PushableHandle>();
                        _move = tmp_pushableHandle.TestPush(_moveDirection);
                        Debug.Log(_move);
                        if (_move)
                        {
                            _state = _trapStateIndex == 0 ? TrapState.Idle : TrapState.Set;
                        }
                        break;
                }
                break;
            case TrapState.Set:
                _kill = false;
                _move = true;
                break;
            case TrapState.Idle:
                _kill = true;
                _move = false;
                break;
                
        }
        return Tuple.Create<DeathType, bool, bool>(_deathType, _kill, _move);

    }

    public bool TestPushable(GameObject _invokingObject) //pushable is asking if it can cross.
    {
        bool _proceed = false;
        switch(_state)
        {
            case TrapState.Crossing:
                _proceed = false;
                break;
            case TrapState.Waiting:
                _proceed = false;
                break;
            default:
                _pushable = _invokingObject;
                _state = TrapState.Waiting;
                _proceed = true;
                break;

        }
        return _proceed;
    }
    void Update()
    {
        if (_state == TrapState.Loading) return;
        switch(_state)
        {
            case TrapState.Set:
                _spriteRenderer.sprite = _sprites[_trapStateIndex];
                break;
            case TrapState.Idle:
                _spriteRenderer.sprite = _sprites[_trapStateIndex];
                if ((_mainPlayer.GetPosition() - _currentTransform.localPosition).magnitude < 0.707)
                {
                    _mainPlayer.Kill(_deathType);
                    _state = TrapState.Loading;
                }
                break;
            case TrapState.Waiting:
                PushableHandle tmp_handle = _pushable.GetComponent<PushableHandle>();
                if ((tmp_handle.GetPosition() - _currentTransform.localPosition).magnitude <= 0.15)
                {
                    _state = TrapState.Crossing;
                }
                break;

        }
    }

}
