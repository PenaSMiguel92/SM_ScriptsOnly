using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayer 
{
    public PlayerState State { get; }
    public int Score { get; }
    public void Kill(DeathType _deathType);
    public void ForceMove(Vector3Int _direction, float _speed);
    public void AddToInventory(PickUpType _pickup);
    public Vector3 GetPosition();
}

public class EventDeath : EventArgs
{
    public DeathType TypeOfDeath;
    public float TimeOfDeath;
    public EventDeath(DeathType _deathtype)
    {
        TypeOfDeath = _deathtype;
        TimeOfDeath = Time.realtimeSinceStartup;
    }
}

public class Player : MonoBehaviour, IPlayer
{
    [SerializeField] private Sprite[] _plrWalk;
    [SerializeField] private Sprite[] _plrBombWalk;
    [SerializeField] private Sprite[] _plrTorchWalk;
    [SerializeField] private Sprite[] _plrStandardDeath;
    [SerializeField] private Sprite[] _plrBurnDeath;
    [SerializeField] private Sprite[] _plrElectricutionDeath;
    [SerializeField] private Sprite[] _plrAcidDeath;
    [SerializeField] private GameObject _droppedBomb;

    private Sprite[] _plrCurrentSprites;
    private Vector3 _gridOffset = new Vector3(0.5f, 0.5f, 0);
    private int _plrScore;
    public int Score {get { return _plrScore; } }
    private PlayerState _plrState = PlayerState.Loading;
    public PlayerState State {get { return _plrState; } }

    private GameControl _mainControl;
    private AudioManager _audioManager;
    private Input _inputManager;

    private Transform _plrTransform;
    private Vector3 _plrPosition;
    private Vector3Int _plrGridPosition;
    private Vector2 _plrInputDirection;
    private Vector3Int _plrMoveDirection;
    private float _plrMoveT;
    private int _plrFacingDirection; //direction, player is facing in angle;
    private const int _PLRFRAMERATE = 2;
    private float _plrMoveSpeed = 4f;
    private int _plrLastFrame; //animation frame.
    private IEnumerator _plrAnimation;
    private double _plrTimerRT; //timer real time since event.
    private float _plrTimer1; //timing frames
    //gameplay variables
    private List<PickUpType> _plrInventory = new List<PickUpType>();
    private PickUpType _plrCurrentSelection = PickUpType.None;
    //private string[] tools; //pickup bomb and torch, select which one is active.
    private PickUpType _plrSelectedTool; //selected tool, index above.
    public static Player Main;
    public event EventHandler<EventDeath> onPlayerDeath;
    public event EventHandler onPlayerAnimationEnd;
    void Awake()
    {
        Main = this;
    }
    void Start()
    {
        _mainControl = GameControl.Main;
        _audioManager = AudioManager.Main;
        _inputManager = Input.Main;
        _mainControl.onGameStart += OnGameStart;

    }
    void OnGameStart(object _sender, EventArgs _e)
    {
        _plrTransform = gameObject.GetComponent<Transform>();
        _plrPosition = _plrTransform.localPosition;
        _plrGridPosition = _mainControl.GetGridPosition(_plrPosition);
        _plrScore = 0;
        onPlayerDeath += Death;
        _inputManager.onPlayerWalk += OnPlayerMove;
        _inputManager.onPlayerInteract += OnPlayerInteract;
        _inputManager.onPlayerInventorySelect += OnPlayerInventorySelect;

        _plrState = PlayerState.Idle;
    }
    public void Kill(DeathType _deathtype)
    {
        _plrState = PlayerState.Death;
        onPlayerDeath?.Invoke(this, new EventDeath(_deathtype));
    }
    public void Death(object _sender, EventDeath _deathevent)
    {
        switch(_deathevent.TypeOfDeath)
        {
            case DeathType.Standard:
                _plrCurrentSprites = _plrStandardDeath;
                break;
            case DeathType.Electricution:
                _plrCurrentSprites = _plrElectricutionDeath;
                break;
            case DeathType.Burn:
                _plrCurrentSprites = _plrBurnDeath;
                break;
            case DeathType.Acid:
                _plrCurrentSprites = _plrAcidDeath;
                break;
        }
        _audioManager.PlaySound(SoundType.Death, _plrPosition);
        if (_plrAnimation != null) StopCoroutine(_plrAnimation);
        _plrAnimation = PlayAnimation(false);
        StartCoroutine(_plrAnimation);
    }

    void SetWalkToSelectedInventory()
    {
        switch(_plrCurrentSelection)
        {
            case PickUpType.None:
                _plrCurrentSprites = _plrWalk;
                break;
            case PickUpType.Bomb:
                _plrCurrentSprites = _plrBombWalk;
                break;
            case PickUpType.Torch:
                _plrCurrentSprites = _plrTorchWalk;
                break;
        }

    }
    void OnPlayerInventorySelect(object _sender, InventorySelect _e)
    {
        List<PickUpType> tmp_selectables =  new List<PickUpType>{ PickUpType.None, PickUpType.Bomb, PickUpType.Torch };
        int _curIndex = tmp_selectables.IndexOf(_plrCurrentSelection);
        int _dir = _e.Direction == InventoryDirection.Left ? -1 : 1;
        Debug.Log(_curIndex);
        bool _found = false;
        while (!_found)
        {
            _curIndex += _dir;
            Debug.Log(_curIndex);
            _curIndex = _curIndex < 0 ? tmp_selectables.Count - 1 : _curIndex;
            _curIndex = _curIndex >= tmp_selectables.Count ? 0 : _curIndex;
            if (_plrInventory.Contains(tmp_selectables[_curIndex])) break;
            if (tmp_selectables[_curIndex] == PickUpType.None) break;
        }
        _plrCurrentSelection = tmp_selectables[_curIndex];
    }
    void OnPlayerInteract(object _sender, EventArgs _e)
    {
        if (_plrState == PlayerState.Death) return;
        if (_plrState != PlayerState.Idle) return;
        
        switch(_plrCurrentSelection)
        {
            case PickUpType.Bomb:
                Vector3 tmp_placeLocation = _plrGridPosition + _gridOffset + new Vector3Int(Mathf.RoundToInt(Mathf.Cos(Mathf.Deg2Rad * _plrFacingDirection)), Mathf.RoundToInt(Mathf.Sin(Mathf.Deg2Rad * _plrFacingDirection)), 0);
                GameObject _bombObj = Instantiate(_droppedBomb, tmp_placeLocation, Quaternion.Euler(0, 0, 0));
                _bombObj.GetComponent<BombHandle>().SpawnTime = Time.realtimeSinceStartup;
                int _bombIndex = _plrInventory.IndexOf(PickUpType.Bomb);
                _plrInventory.RemoveAt(_bombIndex);
                _plrCurrentSelection = PickUpType.None;
                break;
        }

    }
    void TestNextLocation(Vector3Int _location, Vector3Int _moveDirection)
    {
        bool _proceed = true;
        List<TilemapUse> tmp_tilemaps = new List<TilemapUse> { TilemapUse.Foreground, TilemapUse.Moveables };
        foreach (TilemapUse tmp_tilemap in tmp_tilemaps)
        {
            if (!_mainControl.HasTile(_location, tmp_tilemap)) continue;
            GameObject tmp_spot_obj = _mainControl.GetInstantiatedObject(_location, tmp_tilemap);
            if (tmp_spot_obj == null) continue;
            switch (tmp_spot_obj.tag)
            {        
                case "pickup":
                    _proceed = true;
                    break;
                case "push":
                    _proceed = tmp_spot_obj.GetComponent<IPushable>().TestPush(_moveDirection);
                    break;
                case "door":
                    _proceed = tmp_spot_obj.GetComponent<IDoor>().TestDoor(_plrInventory);
                    break;
                case "switch":
                    _proceed = tmp_spot_obj.GetComponent<TrapSwitchHandle>().TestTrapSwitch(gameObject, _moveDirection);
                    break;
                case "trap":
                    //ask object whether it is currently active, what type of death to initiate, and then when or not player can continue moving.
                    var TrapStatus = tmp_spot_obj.GetComponent<TrapHandle>().TestTrap(_moveDirection, gameObject);
                    _proceed = TrapStatus.Item3;
                    if (TrapStatus.Item2)
                    {
                        Kill(TrapStatus.Item1);
                    }        
                    break;
                case "finish":
                    _mainControl.FinishLevel();
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
            _plrMoveDirection = _moveDirection;
            _plrGridPosition += _plrMoveDirection;
            if (_plrAnimation != null) StopCoroutine(_plrAnimation);
            _plrAnimation = PlayAnimation(true);
            StartCoroutine(_plrAnimation);
            _plrMoveSpeed = 4f;
            _plrState = PlayerState.Walking;
        }
    }
    void OnPlayerMove(object _sender, InputValues _e)
    {
        if (_plrState == PlayerState.Death) return;
        if (_plrState != PlayerState.Idle) return;
        if (_e.PlayerDirection.magnitude > 1) return;
        if (_e.PlayerDirection.magnitude < 0.5) return;
        _plrInputDirection = _e.PlayerDirection; //Only up, right, left or down allowed. 
        _plrFacingDirection = Direction2Deg(_plrInputDirection.y, _plrInputDirection.x);
        Vector3Int tmp_plrMoveDirection = new Vector3Int(Mathf.RoundToInt(Mathf.Cos(Mathf.Deg2Rad * _plrFacingDirection)), Mathf.RoundToInt(Mathf.Sin(Mathf.Deg2Rad * _plrFacingDirection)), 0);
        Vector3Int tmp_testingLocation = _plrGridPosition + tmp_plrMoveDirection;
        TestNextLocation(tmp_testingLocation, tmp_plrMoveDirection);

    }
    int Direction2Deg(float y, float x)
    {
        float degrees = Mathf.Rad2Deg * Mathf.Atan2(y,x);
        int result = Mathf.RoundToInt(degrees);

        return result;
    }
    public Vector3 GetPosition()
    {
        return _plrTransform.localPosition;
    }
    Vector3 UpdatePosition(Vector3 _position)
    {
        _plrTransform.localPosition = _position;
        return _plrTransform.localPosition;
    }
    IEnumerator PlayAnimation(bool _loop)
    {
        bool end_anim = false;
        const int FRAME_RATE = _PLRFRAMERATE; //how many frames per animation frame.
        int cur_frame = (_plrLastFrame>_plrCurrentSprites.Length - 1) ? 0 : _plrLastFrame;
        int timer1 = 0;
        while (!end_anim)
        {
            timer1 += 1;
            if (timer1 > FRAME_RATE)
            {
                timer1 = 0;
                cur_frame += 1;
                if (cur_frame > _plrCurrentSprites.Length - 1)
                {
                    if (!_loop || _plrState == PlayerState.Death)
                    {
                        break;
                    }
                    cur_frame = 0;
                }

            }
            _plrLastFrame = cur_frame;
            UpdateSprite( _plrCurrentSprites[cur_frame], new Vector3(0, 0, _plrFacingDirection - 90));
            yield return new WaitForEndOfFrame();
        }
        onPlayerAnimationEnd?.Invoke(this, EventArgs.Empty);
    }

    void UpdateSprite(Sprite sprite, Vector3 rotation)
    {
        _plrTransform.localEulerAngles = rotation;
        SpriteRenderer sp_rend = gameObject.GetComponent<SpriteRenderer>();
        sp_rend.sprite = sprite;
        return;
    }
    public void AddToInventory(PickUpType _pickup)
    {
        switch (_pickup)
        {
            case PickUpType.Coin:
                _plrScore += 100;
                _audioManager.PlaySound(SoundType.Pickup, _plrPosition);
                _mainControl.SetTile(_plrGridPosition, null, TilemapUse.Foreground);
                break;
            case PickUpType.CoinChest:
                _plrScore += 500;
                _audioManager.PlaySound(SoundType.Chest, _plrPosition);
                _mainControl.SetTile(_plrGridPosition, null, TilemapUse.Foreground);
                break;
            default:
                _plrInventory.Add(_pickup);
                if (_pickup == PickUpType.Bomb || _pickup == PickUpType.Torch)
                {
                    _plrCurrentSelection = _pickup;
                }
                _audioManager.PlaySound(SoundType.Pickup, _plrPosition);
                _mainControl.SetTile(_plrGridPosition, null, TilemapUse.Foreground);
                break;
        }
    }

    void TestNextForcedLocation(Vector3Int _location, Vector3Int _moveDirection, float _speed)
    {
        bool _proceed = true;
        List<TilemapUse> tmp_tilemaps = new List<TilemapUse> { TilemapUse.Foreground, TilemapUse.Moveables };
        foreach (TilemapUse tmp_tilemap in tmp_tilemaps)
        {
            if (!_mainControl.HasTile(_location, tmp_tilemap)) continue;
            GameObject tmp_spot_obj = _mainControl.GetInstantiatedObject(_location, tmp_tilemap);
            if (tmp_spot_obj == null) continue;
            switch (tmp_spot_obj.tag)
            {        
                case "pickup":
                    _proceed = true;
                    break;
                case "push":
                    _proceed = tmp_spot_obj.GetComponent<PushableHandle>().TestPush(_moveDirection);
                    break;
                case "door":
                    _proceed = tmp_spot_obj.GetComponent<IDoor>().TestDoor(_plrInventory);
                    break;
                case "switch":
                    _proceed = tmp_spot_obj.GetComponent<TrapSwitchHandle>().TestTrapSwitch(gameObject, _moveDirection);
                    break;
                case "trap":
                    //ask object whether it is currently active, what type of death to initiate, and then when or not player can continue moving.
                    var TrapStatus = tmp_spot_obj.GetComponent<TrapHandle>().TestTrap(_moveDirection, gameObject);
                    _proceed = TrapStatus.Item3;
                    if (TrapStatus.Item2)
                    {
                        Kill(TrapStatus.Item1);
                    }        
                    break;
                case "finish":
                    _mainControl.FinishLevel();
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
            _plrMoveDirection = _moveDirection;
            _plrGridPosition += _plrMoveDirection;
            if (_plrAnimation != null) StopCoroutine(_plrAnimation);
            _plrMoveSpeed = _speed;
            _plrState = PlayerState.Sliding;
        }
    }

    public void ForceMove(Vector3Int _direction, float _speed)
    {
        if (_plrState == PlayerState.Death) return;
        if (_plrState != PlayerState.Idle) return;
        
        _plrFacingDirection = Direction2Deg(_direction.y, _direction.x);
        Vector3Int tmp_plrMoveDirection = new Vector3Int(Mathf.RoundToInt(Mathf.Cos(Mathf.Deg2Rad * _plrFacingDirection)), Mathf.RoundToInt(Mathf.Sin(Mathf.Deg2Rad * _plrFacingDirection)), 0);
        Vector3Int tmp_testingLocation = _plrGridPosition + tmp_plrMoveDirection;
        TestNextForcedLocation(tmp_testingLocation, tmp_plrMoveDirection, _speed);
    }

    void PlayerLerpMove()
    {
        _plrMoveT = Mathf.Min(1, _plrMoveT + (Time.deltaTime*_plrMoveSpeed));
        UpdatePosition(Vector3.Lerp(_plrPosition, _plrGridPosition + _gridOffset, _plrMoveT)); 
        if (_plrMoveT == 1)
        {
            _plrMoveT = 0f;
            _plrPosition = UpdatePosition( _plrGridPosition + _gridOffset);
            _plrState = PlayerState.Idle;
        }
    }

    void Update()
    {
        if (_plrState == PlayerState.Loading) return;
        if (_plrState == PlayerState.Death) return;
        SetWalkToSelectedInventory();
        switch (_plrState)
        {
            case PlayerState.Idle:
                if (_plrAnimation != null) StopCoroutine(_plrAnimation);
                UpdateSprite(_plrCurrentSprites[0], new Vector3(0, 0, _plrFacingDirection - 90));
                break;
            case PlayerState.Walking:
                PlayerLerpMove();
                break;
            case PlayerState.Sliding:
                PlayerLerpMove();
                break;
        }
    }

}
