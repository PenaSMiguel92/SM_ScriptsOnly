using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum ShootDirection { Up, Right, Down, Left };
public class BudaStatueHandle : MonoBehaviour, IPushable, IStateChange<TrapType, PushState>
{
    [SerializeField] TrapType _type;
    [SerializeField] ShootDirection _shootDir;
    [SerializeField] Sprite[] _sprites;
    bool _crossingExit = false;
    bool _crossingEnter = false;
    bool _crossingMoving = false;
    private Vector3 _gridOffset = new Vector3(0.5f, 0.5f, 0);
    private float _moveSpeed = 4.25f;
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
    SpriteRenderer _curSpriteRenderer;
    int _curFrame;
    const int _FRAMERATE = 2;
    //public event EventHandler onAnimationEnd;
    public TrapType Type { get { return _type; } }
    public bool CrossingExit {get { return _crossingExit; } set { _crossingExit = value; } }
    public bool CrossingEnter {get { return _crossingEnter; } set { _crossingEnter = value; } }
    public bool CrossingMoving {get { return _crossingMoving; } set { _crossingMoving = value; } }
    public PushState State {get { return _state; } set { _state = value; } }
    public Vector3Int MoveDirection { get { return _dir; } }
    public Vector3Int TileLocation { get { return _tileLocation; } }

    void Start()
    {
        _mainControl = GameControl.Main;
        _audioManager = AudioManager.Main;
        _currentTransform = gameObject.GetComponent<Transform>();
        _curSpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
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
    void UpdateSprite( Sprite sprite, Vector3 rotation)
    {
        _currentTransform.localEulerAngles = rotation;
        _curSpriteRenderer.sprite = sprite;
        return;
    }
    IEnumerator PlayAnimation(bool _loop)
    {
        bool end_anim = false;
        const int FRAME_RATE = _FRAMERATE; //how many frames per animation frame.
        int cur_frame = (_curFrame>_sprites.Length - 1) ? 0 : _curFrame;
        int timer1 = 0;
        while (!end_anim)
        {
            timer1 += 1;
            if (timer1 > FRAME_RATE)
            {
                timer1 = 0;


                cur_frame += 1;
                //cur_frame = cur_frame > (_plrCurrentSprites.Length - 1) ? _plrCurrentSprites.Length-1 : cur_frame;
                if (cur_frame > _sprites.Length - 1)
                {
                    cur_frame = 0;
                    if (!_loop)
                    {
                        break;
                    }
                }

            }
            _curFrame = cur_frame;
            UpdateSprite( _sprites[cur_frame], new Vector3(0, 0, 0));
            yield return new WaitForEndOfFrame();
        }
        //onAnimationEnd?.Invoke(this, EventArgs.Empty);
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
        if (_state == PushState.Moving) return true;
        bool _proceed = true;
        List<TilemapUse> tmp_tilemaps = new List<TilemapUse> { TilemapUse.Foreground, TilemapUse.Moveables};
        foreach (TilemapUse tmp_tilemap in tmp_tilemaps)
        {
            if (!_mainControl.HasTile(_gridPos + _plrMoveDirection, tmp_tilemap)) continue;
            GameObject tmp_spot_obj = _mainControl.GetInstantiatedObject(_gridPos + _plrMoveDirection, tmp_tilemap);
            if (tmp_spot_obj == null) continue;
            switch (tmp_spot_obj.tag)
            {
                case "switch":
                    _proceed = tmp_spot_obj.GetComponent<TrapSwitchHandle>().TestTrapSwitch(gameObject, _plrMoveDirection);
                    break;
                case "trap":
                    _proceed = tmp_spot_obj.GetComponent<TrapHandle>().TestPushable(gameObject);
                    break;
                case "pickup":
                    _proceed = true;
                    break;
                case "mover":
                    _proceed = true;
                    break;
                default:
                    _proceed = false;
                    break;
            }
        }

        if (_proceed)
        {
            _audioManager.PlaySound(SoundType.Push, _positionTrack);
            _moveT = 0;
            _gridPos += _plrMoveDirection;
            _dir = _plrMoveDirection;
            _moveSpeed = 4.25f;
            _state = PushState.Moving;

        }
        return _proceed;
    }
    public bool ForcePush(Vector3Int _moveDirection, float _speed)
    {
        if (_state == PushState.Moving) return true;
        bool _proceed = true;
        List<TilemapUse> tmp_tilemaps = new List<TilemapUse> { TilemapUse.Foreground, TilemapUse.Moveables};
        foreach (TilemapUse tmp_tilemap in tmp_tilemaps)
        {
            if (!_mainControl.HasTile(_gridPos + _moveDirection, tmp_tilemap)) continue;
            GameObject tmp_spot_obj = _mainControl.GetInstantiatedObject(_gridPos + _moveDirection, tmp_tilemap);
            if (tmp_spot_obj == null) continue;
            switch (tmp_spot_obj.tag)
            {
                case "switch":
                    _proceed = tmp_spot_obj.GetComponent<TrapSwitchHandle>().TestTrapSwitch(gameObject, _moveDirection);
                    break;
                case "trap":
                    _proceed = tmp_spot_obj.GetComponent<TrapHandle>().TestPushable(gameObject);
                    break;
                case "pickup":
                    _proceed = true;
                    break;
                case "mover":
                    _proceed = true;
                    break;
                default:
                    _proceed = false;
                    break;
            }
        }

        if (_proceed)
        {
            //_audioManager.PlaySound(SoundType.Push, _positionTrack);
            _moveT = 0;
            _gridPos += _moveDirection;
            _dir = _moveDirection;
            _moveSpeed = _speed;
            _state = PushState.Moving;

        }
        return _proceed;
    }
    void Update()
    {
        if (_state == PushState.Loading || _state == PushState.Idle) return;
        switch(_state)
        {
            case PushState.Set:
                _mainControl.SetTile(_gridPos - _dir, null, TilemapUse.Moveables);
                break;
            case PushState.Moving:
                _moveT = Mathf.Min(1, _moveT + (Time.deltaTime * _moveSpeed));
                UpdatePosition(Vector3.Lerp(_positionTrack, _gridPos + _gridOffset, _moveT));//_positionTrack + (Time.deltaTime * new Vector3(_dir.x, _dir.y, 0) * _moveSpeed));
                if (_moveT == 1)//((_positionTrack - (_gridPos + new Vector3(0.5f, 0.5f, 0))).magnitude <= 0.05)
                {
                    _positionTrack = UpdatePosition(_gridPos + _gridOffset);
                    _state = PushState.Idle;
                    _mainControl.MoveTile(_gridPos - _dir, _gridPos, TilemapUse.Moveables);
                }
                break;

        }
    }
    

}
