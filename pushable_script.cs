using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum PushState {Loading, Idle, Moving, Crossing, Set}
public interface IPushable
{
    public int IdUse { get; set; }
    public PushState State { get; }
    public TrapType Type { get; }
    public Vector3Int MoveDirection { get; }
    public Vector3Int TileLocation { get; }
    public bool TestPush(Vector3Int _plrMoveDirection);
}

public class pushable_script : MonoBehaviour, IPushable
{
    [SerializeField] TrapType _type;
    int _idUse = 0; //Note that it is set by another class.
    PushState _state = PushState.Loading;
    Vector3Int _dir;
    Vector3Int _tileLocation;
    TileBase _pushTile;
    Vector3Int _gridPos;
    Vector3 _positionTrack;
    GameObject _grid;
    GameObject _instObj;
    Transform _currentTransform;

    GameControl _mainControl;
    AudioManager _audioManager;

    public TrapType Type { get { return _type; } }
    public int IdUse { get { return _idUse; } set { _idUse = value; } }
    public PushState State {get { return _state; } }
    public Vector3Int MoveDirection { get { return _dir; } }
    public Vector3Int TileLocation { get { return _tileLocation; } }

    void Start()
    {
        _mainControl = GameControl.Main;
        _audioManager = AudioManager.Main;
        _currentTransform = gameObject.GetComponent<Transform>();
        _mainControl.onGameStart += OnGameStart;
    }

    void OnGameStart(object _sender, EventArgs _e)
    {
        _positionTrack = _currentTransform.localPosition;
        _gridPos = _mainControl.GetGridPosition(_positionTrack);
        _state = PushState.Idle;
    }

    void Update()
    {
        if (_state == PushState.Loading || _state == PushState.Idle) return;
        if (_state == PushState.Set) Destroy(gameObject);
        if (_state == PushState.Moving)
        {
            _positionTrack = UpdatePosition(_positionTrack + (Time.deltaTime * new Vector3(_dir.x, _dir.y, 0) * 2f));
            if ((_positionTrack - (_gridPos + new Vector3(0.5f, 0.5f, 0))).magnitude <= 0.05)
            {
                _positionTrack = UpdatePosition(_gridPos + new Vector3(0.5f, 0.5f, 0));
                _state = PushState.Idle;
                TileBase _pushTile = _mainControl.GetTile(_gridPos-_dir);
                _mainControl.SetTile(_gridPos, _pushTile);
                _mainControl.SetTile(_gridPos-_dir, null);
            }
        }
    }
    public bool TestPush(Vector3Int _plrMoveDirection)
    {
        if (_state != PushState.Idle) return false;
        bool _proceed = false;
        if (_mainControl.HasTile(_gridPos + _plrMoveDirection))
        {

        }
        else
        {
            _proceed = true;
        }

        if (_proceed)
        {
            _state = PushState.Moving;
            
            _gridPos += _plrMoveDirection;
            _dir = _plrMoveDirection;

        }
        return _proceed;
    }

    // IEnumerator MoveAsDirected()
    // {
    //     bool endLoop = false;
    //     bool _set = false;
    //     while (!endLoop)
    //     {
    //         if (!_set)
    //         {
    //             _set = true;
    //             _gridPos = _mainControl.GetGridPosition(_positionTrack) + _dir;
    //             _instObj = _mainControl.GetInstantiatedObject(_gridPos);
    //             _audioManager.PlaySound(SoundType.Push, _positionTrack);
    //         }
    //         _positionTrack = UpdatePosition(_positionTrack + (Time.deltaTime * new Vector3(_dir.x, _dir.y, 0) * 2f));
    //         if ((_positionTrack - (_gridPos + new Vector3(0.5f, 0.5f, 0))).magnitude <= 0.05)
    //         {
    //             _positionTrack = UpdatePosition(_gridPos + new Vector3(0.5f, 0.5f, 0));
    //             _move = false;
    //             _set = false;
    //             if (_instObj != null)
    //             {
    //                 if (_instObj.tag == "obstacle")
    //                 {
    //                     if (!_instObj.GetComponent<state_chg>().State)
    //                     {
    //                         if (_instObj.GetComponent<state_chg>().Type == _type) //gamecontrol should update the tiles.
    //                         {
    //                             _instObj.GetComponent<state_chg>().State = true;
    //                             _instObj.GetComponent<state_chg>().Crossable = true;
                                
    //                             GameObject[] objects = GameObject.FindGameObjectsWithTag("trap");
    //                             foreach (GameObject obj in objects)
    //                             {
    //                                 if (obj.GetComponent<trap_state>().Type == _instObj.GetComponent<state_chg>().Type)
    //                                 {
    //                                     obj.GetComponent<trap_state>().TrapEnabled = false;
    //                                 }
    //                             }
    //                             _audioManager.PlaySound(SoundType.SetPushable, _positionTrack);
    //                             _mainControl.SetTile(_tileLocation, null);
    //                         }
    //                         else
    //                         {
    //                             _crossing = true;
    //                             _instObj.GetComponent<state_chg>().Crossing = true;
    //                             _instObj.GetComponent<state_chg>().storeTileandObject(new TileandObject(_tileLocation, gameObject)); //make sure to make player set ruleTile.
    //                         }
    //                     }
    //                     else
    //                     {
    //                         _crossing = true;
    //                         _instObj.GetComponent<state_chg>().Crossing = true;
    //                         _instObj.GetComponent<state_chg>().storeTileandObject(new TileandObject(_tileLocation, gameObject)); //make sure to make player set ruleTile.
    //                     }
    //                 }
    //             }
    //             else
    //             {
    //                 _mainControl.SetTile(_gridPos, _mainControl.GetTile(_tileLocation));
    //                 _mainControl.SetTile(_tileLocation, null);
                    
    //             }
    //             endLoop = true;
    //         }
    //         yield return new WaitForEndOfFrame();
    //     }
    // }

    public Vector3 UpdatePosition(Vector3 _newPosition)
    {
        _currentTransform.localPosition = _newPosition;
        return _currentTransform.localPosition;
    }
}
