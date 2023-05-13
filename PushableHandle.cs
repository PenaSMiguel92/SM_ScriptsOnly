using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum PushState {Loading, Idle, Moving, Crossing, EndCrossing, Set}
public interface IPushable
{
    public int IdUse { get; set; }
    public PushState State { get; set; }
    public Vector3Int MoveDirection { get; }
    public Vector3Int TileLocation { get; }
    public bool CrossingExit { get; set; }
    public bool CrossingEnter { get; set; }
    public bool TestPush(Vector3Int _plrMoveDirection);
    public Vector3 GetPosition();
}
public class PushableHandle : MonoBehaviour, IPushable, IStateChange<TrapType, PushState>
{
    [SerializeField] TrapType _type;
    int _idUse = 0; //Note that it is set by another class.
    bool _crossingExit = false;
    bool _crossingEnter = false;
    private const float _MOVESPEED = 4f;
    float _moveT = 0f; //for Vector3.Lerp
    PushState _state = PushState.Loading;
    Vector3Int _dir;
    Vector3Int _tileLocation;
    Vector3Int _gridPos;
    Vector3 _positionTrack;
    GameObject _grid;
    GameObject _instObj;
    Transform _currentTransform;

    GameControl _mainControl;
    AudioManager _audioManager;

    public TrapType Type { get { return _type; } }
    public int IdUse { get { return _idUse; } set { _idUse = value; } }
    public bool CrossingExit {get { return _crossingExit; } set { _crossingExit = value; } }
    public bool CrossingEnter {get { return _crossingEnter; } set { _crossingEnter = value; } }
    public PushState State {get { return _state; } set { _state = value; } }
    public Vector3Int MoveDirection { get { return _dir; } }
    public Vector3Int TileLocation { get { return _tileLocation; } }

    void Start()
    {
        _mainControl = GameControl.Main;
        _audioManager = AudioManager.Main;
        _currentTransform = gameObject.GetComponent<Transform>();
        _mainControl.onGameStart += OnGameStart;
        if (_mainControl.State == GameState.LevelPlay)
        {
            OnGameStart(this, EventArgs.Empty);
        }
    }

    void OnGameStart(object _sender, EventArgs _e)
    {
        _positionTrack = _currentTransform.localPosition;
        _gridPos = _mainControl.GetGridPosition(_positionTrack);
        _state = PushState.Idle;
    }
    
    public Vector3 GetPosition()
    {
        return _currentTransform.localPosition;
    }

    public Vector3 UpdatePosition(Vector3 _newPosition)
    {
        _currentTransform.localPosition = _newPosition;
        return _currentTransform.localPosition;
    }
    public bool TestPush(Vector3Int _plrMoveDirection)
    {
        if (_state != PushState.Idle) return false;
        bool _proceed = false;
        if (_mainControl.HasTile(_gridPos + _plrMoveDirection))
        {
            GameObject tmp_spot_obj = _mainControl.GetInstantiatedObject(_gridPos + _plrMoveDirection);
            if (tmp_spot_obj != null)
            {
                switch(tmp_spot_obj.tag)
                {
                    case "wall":
                        _proceed = false;
                        break;
                    case "switch":
                        _proceed = tmp_spot_obj.GetComponent<TrapSwitchHandle>().TestTrapSwitch(gameObject, _plrMoveDirection);
                        break;
                    case "trap":
                        _proceed = tmp_spot_obj.GetComponent<TrapHandle>().TestPushable(gameObject);
                        break;
                    case "pickup":
                        _proceed = true;
                        break;
                    default:
                        _proceed = false;
                        break;
                }
            }
            else
            {
                _proceed = true;
            }
        }
        else
        {
            _proceed = true;
        }

        if (_proceed)
        {
            _audioManager.PlaySound(SoundType.Push, _positionTrack);
            _state = PushState.Moving;
            _moveT = 0;
            _gridPos += _plrMoveDirection;
            _dir = _plrMoveDirection;

        }
        return _proceed;
    }

    void Update()
    {
        if (_state == PushState.Loading || _state == PushState.Idle) return;
        switch(_state)
        {
            case PushState.Set:
                _mainControl.SetTile(_gridPos - _dir, null);
                break;
            case PushState.Moving:
                _moveT = Mathf.Min(1, _moveT + (Time.deltaTime * _MOVESPEED));
                UpdatePosition(Vector3.Lerp(_positionTrack, _gridPos + new Vector3(0.5f, 0.5f, 0), _moveT));//_positionTrack + (Time.deltaTime * new Vector3(_dir.x, _dir.y, 0) * _moveSpeed));
                if (_moveT == 1)//((_positionTrack - (_gridPos + new Vector3(0.5f, 0.5f, 0))).magnitude <= 0.05)
                {
                    _positionTrack = UpdatePosition(_gridPos + new Vector3(0.5f, 0.5f, 0));
                    _state = PushState.Idle;
                    //Note that moving tile will delete this gameobject, so game conntrol needs to be the one that does it.
                    if (_crossingExit && !_crossingEnter)
                    {
                        _mainControl.RestoreTile(_gridPos - _dir, _gridPos); //move from overlapping tilemap to foreground tilemap;
                    }
                    else if (!CrossingExit && _crossingEnter)
                    {
                        _mainControl.StoreTile(_gridPos - _dir, _gridPos); //move from foreground tilemap to overlapping tilemap;
                    }
                    else
                    {
                        _mainControl.MoveTile(_gridPos - _dir, _gridPos); //move within foreground tilemap.
                    }
                    
                }
                break;

        }
    }
    

}
