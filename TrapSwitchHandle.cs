using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public interface ITrapSwitch
{
    public bool TestTrapSwitch(GameObject _invokingObject, Vector3Int _moveDirection);
    // public void StoreTile(TileBase _targetTile);
}

public class TrapSwitchHandle : MonoBehaviour, IStateChange<TrapType, TrapState>, ITrapSwitch 
{
    [SerializeField] private Sprite[] _sprites; //sprites to use.
    [SerializeField] private TrapType _type;
    private TrapState _state = TrapState.Loading;
    public TrapState State {get { return _state; } }
    public TrapType Type {get { return _type; } }
    private SpriteRenderer _spriteRenderer;
    private Vector3Int _tileLoc;
    private Transform _curTransform;
    private GameObject _pushable;
    private TileBase _pushableTile;
    private GameControl _mainControl;
    private Player _mainPlayer;
    private AudioManager _audioManager;
    

    // Start is called before the first frame update
    void Start()
    {
        _mainControl = GameControl.Main;
        _mainPlayer = Player.Main;
        _audioManager = AudioManager.Main;
        _mainControl.onGameStart += OnGameStart;
        _curTransform = gameObject.GetComponent<Transform>();
        if (_mainControl.State == GameState.LevelPlay)
        {
            OnGameStart(this, EventArgs.Empty);
        }
    }
    void OnGameStart(object _sender, EventArgs _e)
    {
        _state = TrapState.Idle;
        _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        _spriteRenderer.sprite = _sprites[0];
        _tileLoc = _mainControl.GetGridPosition(_curTransform.localPosition);
    }
    // Update is called once per frame
    
    public bool TestTrapSwitch( GameObject _invokingObject, Vector3Int _moveDirection)
    {
        bool _proceed = false;
        GameObject tmp_pushable;
        PushableHandle tmp_pushableHandle;

        switch(_invokingObject.tag)
        {
            case "enemy":
                _proceed = _state == TrapState.Set;
                break;
            case "player":
                if (_state == TrapState.Crossing)
                {
                    tmp_pushable = _mainControl.GetInstantiatedObject(_tileLoc,TilemapUse.Moveables);
                    tmp_pushableHandle = tmp_pushable.GetComponent<PushableHandle>();
                    _proceed = tmp_pushableHandle.TestPush(_moveDirection);
                    if (_proceed)
                    {
                        _state = TrapState.Set;
                    }
                }
                else
                {
                    if (_type == TrapType.ElectricalBox)
                    {
                        _proceed = true;
                    } 
                    else
                    {
                        _proceed = _state == TrapState.Set;
                    }
                }
                break;
            default:
                tmp_pushableHandle = _invokingObject.GetComponent<PushableHandle>();
                if (_state != TrapState.Set && tmp_pushableHandle.Type != _type) return false; //if not set (state), thus not crossable, and not the same type, then prevent movement.
            
                if (_state == TrapState.Set)
                {
                    _state = TrapState.Crossing;
                    _proceed = true;
                    //_mainControl.StoreTile(tmp_pushable.TileLocation, _tileLoc);
                }
                else
                {
                    
                    if (tmp_pushableHandle.Type == _type)        
                    {
                        _proceed = true;
                        _pushable = _invokingObject;
                        _state = TrapState.Waiting;
                    }
                }
                break;
               
        }
        return _proceed;
    }
    // public void StoreTile(TileBase _targetTile)
    // {
    //     _pushableTile = _targetTile;
    // }
    void HandleState()
    {
        if (_state == TrapState.Loading) return;
        switch (_state)
        {
            case TrapState.Idle:
                if (_type != TrapType.ElectricalBox) break;
                if ((_mainPlayer.GetPosition() - _curTransform.localPosition).magnitude <= 0.15)
                {
                    _spriteRenderer.sprite = _sprites[1];
                    _mainControl.InvokeTrapStateChange(true);
                    _state = TrapState.PlayerStand;
                }
                break;
            case TrapState.PlayerStand:
                if (_type != TrapType.ElectricalBox) break;
                if ((_mainPlayer.GetPosition() - _curTransform.localPosition).magnitude >= 1)
                {
                    _spriteRenderer.sprite = _sprites[0];
                    _mainControl.InvokeTrapStateChange(false);
                    _state = TrapState.Idle;
                }
                break;
            case TrapState.Crossing:
                if (_type != TrapType.ElectricalBox) break;
                _spriteRenderer.sprite = _sprites[1];
                break;
            case TrapState.Set:
                _spriteRenderer.sprite = _sprites[_sprites.Length - 1];
                break;
            case TrapState.Waiting:
                PushableHandle tmp_handle = _pushable.GetComponent<PushableHandle>();
                if ((tmp_handle.GetPosition() - _curTransform.localPosition).magnitude <= 0.15)
                {
                    tmp_handle.State = PushState.Set;
                    _state = TrapState.Set;
                    if (_type == TrapType.ElectricalBox)
                    {
                        _mainControl.InvokeTrapStateChange(true);
                    }
                }
                break;


        }
    }
    void Update()
    {
        HandleState();
    }
}
