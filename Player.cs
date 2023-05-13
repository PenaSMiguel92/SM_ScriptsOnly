using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum PlayerState { Loading, Idle, Walking, Death };
public interface IPlayer 
{
    public PlayerState State { get; }
    public int Score { get; }
    public void Kill(DeathType _deathType);
    public void ForceMove(Vector3Int _direction, float _speed);
    public void AddToInventory(PickUpEnum _pickup);
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
    private Sprite[] _plrCurrentSprites;
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
    private const int _PLRFRAMERATE = 8;
    private const float _PLRMOVESPEED = 4f;
    private int _plrLastFrame; //animation frame.
    private IEnumerator _plrAnimation;
    private double _plrTimerRT; //timer real time since event.
    private float _plrTimer1; //timing frames
    //gameplay variables

    private List<PickUpEnum> _plrInventory = new List<PickUpEnum>();
    //private string[] tools; //pickup bomb and torch, select which one is active.
    private PickUpEnum _plrSelectedTool; //selected tool, index above.
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
        _plrAnimation = PlayAnimation(false);
        StartCoroutine(_plrAnimation);
    }

    void OnPlayerMove(object _sender, InputValues _e)
    {
        if (_plrState == PlayerState.Death) return;
        if (_plrState != PlayerState.Idle) return;
        if (_e.PlayerDirection.magnitude > 1)
        {
            return;
        }
        else
        {
            _plrInputDirection = _e.PlayerDirection.normalized; //Only up, right, left or down allowed. 
        }
        bool _proceed = false;
        _plrFacingDirection = Direction2Deg(_plrInputDirection.y, -_plrInputDirection.x);
        Vector3Int tmp_plrMoveDirection = new Vector3Int(Mathf.RoundToInt(Mathf.Cos(Mathf.Deg2Rad * _plrFacingDirection)), Mathf.RoundToInt(Mathf.Sin(Mathf.Deg2Rad * _plrFacingDirection)), 0);
        Vector3Int tmp_testingLocation = _plrGridPosition + tmp_plrMoveDirection;
        if (_mainControl.HasTile(tmp_testingLocation))
        {
            GameObject tmp_spot_obj = _mainControl.GetInstantiatedObject(tmp_testingLocation);
            if (tmp_spot_obj != null)
            {
                switch (tmp_spot_obj.tag)
                {
                    case "wall":
                        _proceed = false;
                        break;
                    case "pickup":
                        _proceed = true;
                        break;
                    case "push":
                        _proceed = tmp_spot_obj.GetComponent<PushableHandle>().TestPush(tmp_plrMoveDirection);
                        break;
                    case "door":
                        _proceed = tmp_spot_obj.GetComponent<DoorHandle>().TestDoor(_plrInventory);
                        break;
                    case "switch":
                        _proceed = tmp_spot_obj.GetComponent<TrapSwitchHandle>().TestTrapSwitch(gameObject, tmp_plrMoveDirection);
                        break;
                    case "trap":
                        //ask object whether it is currently active, what type of death to initiate, and then when or not player can continue moving.
                        var TrapStatus = tmp_spot_obj.GetComponent<TrapHandle>().TestTrap(tmp_plrMoveDirection, gameObject);
                        _proceed = TrapStatus.Item3;
                        if (TrapStatus.Item2)
                        {
                            Kill(TrapStatus.Item1);
                        }
                        
                        break;
                    case "Finish":
                        //finish the level.
                        _proceed = true;
                        _mainControl.FinishLevel();
                        break;
                    default:
                        _proceed = true;
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
            _plrMoveDirection = tmp_plrMoveDirection;
            _plrGridPosition += _plrMoveDirection;
            _plrMoveT = 0f;
            _plrCurrentSprites = _plrWalk;
            _plrAnimation = PlayAnimation(true);
            StartCoroutine(_plrAnimation);
        
            _plrState = PlayerState.Walking;
        }
        
    }
    int Direction2Deg(float y, float x)
    {
        float degrees = Mathf.Rad2Deg * Mathf.Atan2(y,x);
        int result = Mathf.RoundToInt(180f-degrees);

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
                //cur_frame = cur_frame > (_plrCurrentSprites.Length - 1) ? _plrCurrentSprites.Length-1 : cur_frame;
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
    public void AddToInventory(PickUpEnum _pickup)
    {
        switch (_pickup)
        {
            case PickUpEnum.Coin:
                _plrScore += 100;
                _audioManager.PlaySound(SoundType.Pickup, _plrPosition);
                _mainControl.SetTile(_plrGridPosition, null);
                break;
            case PickUpEnum.CoinChest:
                _plrScore += 500;
                _audioManager.PlaySound(SoundType.Chest, _plrPosition);
                _mainControl.SetTile(_plrGridPosition, null);
                break;
            default:
                _plrInventory.Add(_pickup);
                _audioManager.PlaySound(SoundType.Pickup, _plrPosition);
                _mainControl.SetTile(_plrGridPosition, null);
                break;
        }
    }

    public void ForceMove(Vector3Int _direction, float _speed)
    {

    }

    void Update()
    {
        if (_plrState == PlayerState.Loading) return;
        if (_plrState == PlayerState.Death) return;
        switch (_plrState)
        {
            case PlayerState.Idle:
                if (_plrAnimation != null)
                {
                    StopCoroutine(_plrAnimation);
                }
                UpdateSprite(_plrWalk[0], new Vector3(0, 0, _plrFacingDirection - 90));
                break;
            case PlayerState.Walking:
                //_plrPosition = 
                _plrMoveT = Mathf.Min(1, _plrMoveT + (Time.deltaTime*_PLRMOVESPEED));
                UpdatePosition(Vector3.Lerp(_plrPosition, _plrGridPosition + new Vector3(0.5f, 0.5f, 0), _plrMoveT)); //_plrPosition + (Time.deltaTime * new Vector3(_plrMoveDirection.x, _plrMoveDirection.y, 0) * _PLRMOVESPEED));
                if (_plrMoveT == 1) //((_plrPosition - (_plrGridPosition + new Vector3(0.5f, 0.5f, 0))).magnitude <= 0.05)
                {
                    _plrPosition = UpdatePosition( _plrGridPosition + new Vector3(0.5f, 0.5f, 0));
                    _plrState = PlayerState.Idle;
                }
                break;
        }

            
    //             if (!_mainControl.HasTile(_plrGridPosition + _plrMoveDirection))
    //             {
    //                 _plrMoving = true;
    //                 _plrGridPosition += _plrMoveDirection;
    //             }
    //             else
    //             {
    //                 //spot_obj = null;
    //                 //spot_obj = null;
    //                 GameObject spot_obj = _mainControl.GetInstantiatedObject(_plrGridPosition + _plrMoveDirection);
    //                 if (spot_obj != null)
    //                 {
    //                     Vector3 obj_pos = spot_obj.GetComponent<Transform>().localPosition;
    //                     Vector3Int obj_gridpos = _mainControl.GetGridPosition(obj_pos);
    //                     GameObject nextTileObj = _mainControl.GetInstantiatedObject(obj_gridpos + _plrMoveDirection);
    //                     switch (spot_obj.tag)
    //                     {
    //                         case "wall":
    //                             //print("wall");
    //                             return;
    //                         case "pickup":
    //                             //print("pickup");
    //                             _plrMoving = true;
    //                             _plrGridPosition += _plrMoveDirection;
    //                             AddToInventory(spot_obj.GetComponent<pickup_type>().TypeOfPickup);
    //                             return;
                                
    //                         case "push":
    //                             if (_plrMoveDirection.magnitude == 1)
    //                             {
    //                                 if (!spot_obj.GetComponent<pushable_script>().Crossing) //move to spot_obj's location if it is currently crossing and saved to a different tile.
    //                                 {
    //                                     if (nextTileObj != null)
    //                                     {
    //                                         if (nextTileObj.tag == "obstacle" && (int) nextTileObj.GetComponent<state_chg>().Type < 2)
    //                                         {
    //                                             spot_obj.GetComponent<pushable_script>().SetProperties(_plrMoveDirection, true, obj_gridpos);
    //                                             // invokePushObject(spot_obj);
    //                                             _plrMoving = true;
    //                                             _plrGridPosition += _plrMoveDirection;
    //                                         }
    //                                     }
    //                                     else
    //                                     {
    //                                         spot_obj.GetComponent<pushable_script>().SetProperties(_plrMoveDirection, true, obj_gridpos);
    //                                         //invokePushObject(spot_obj);
    //                                         _plrMoving = true;
    //                                         _plrGridPosition += _plrMoveDirection;
    //                                     }
    //                                 }
    //                                 else
    //                                 {
    //                                     _plrMoving = true;
    //                                     _plrGridPosition += _plrMoveDirection;
    //                                 }


    //                             }

    //                             return;
    //                         case "obstacle":
    //                             if (_plrMoveDirection.magnitude == 1)
    //                             {
    //                                 if (spot_obj.tag == "obstacle")
    //                                 {
    //                                     // TrapType type_obstacle = spot_obj.GetComponent<state_chg>().Type;
    //                                     //     if (_plrInventory.Contains(type_obstacle) && type_obstacle >1 && type_obstacle < 6)
    //                                     //     {
    //                                     //         AudioSource.PlayClipAtPoint(audioClips[4], spot_obj.GetComponent<Transform>().localPosition);
    //                                     //         inventory.Remove(type_obstacle + 1);
    //                                     //         //StartCoroutine(playOpenDoorAnimation(spot_obj));
    //                                     //         return;
    //                                     //     }
    //                                     //     if (spot_obj.GetComponent<state_chg>().Crossing)
    //                                     //     {
    //                                     //         if (nextTileObj != null)
    //                                     //         {
    //                                     //             if (nextTileObj.tag != "obstacle")
    //                                     //             {
    //                                     //                 return;
    //                                     //             }
    //                                     //             else
    //                                     //             {
    //                                     //                 if ((int) nextTileObj.GetComponent<state_chg>().Type >= 2 || nextTileObj.GetComponent<state_chg>().Crossing)
    //                                     //                 {
    //                                     //                     return;
    //                                     //                 }
    //                                     //             }
    //                                     //         }
    //                                     //         //print(nextTileObj);
    //                                     //         TileandObject tileAndObject = spot_obj.GetComponent<state_chg>().getTileandObjectBack();
    //                                     //         tileAndObject.pushableObject.GetComponent<pushable_script>().SetProperties(_plrMoveDirection, true, tileAndObject.pushableTileLoc);
    //                                     //         invokePushObject(tileAndObject.pushableObject);
    //                                     //         _plrMoving = true;
    //                                     //         _plrGridPosition += _plrMoveDirection;
    //                                     //         spot_obj.GetComponent<state_chg>().Crossing = false;
    //                                     //         spot_obj.GetComponent<state_chg>().storeTileandObject(new TileandObject(new Vector3Int(), null));
    //                                     //         return;                                                
                                                
    //                                     //     }
    //                                     //     else
    //                                     //     {
    //                                     //         if (!spot_obj.GetComponent<state_chg>().Crossable)
    //                                     //         {
    //                                     //             return;
    //                                     //         }
    //                                     //     }
    //                                     //}
    //                                     //else
    //                                     //{
                                            
    //                                     //}
    //                                     //if (type_obstacle >= 2 && type_obstacle <= 5 && inventory.Contains(type_obstacle + 1)) //key door of any color
    //                                     //{
                                            
    //                                     //}
    //                                 }
    //                                 _plrMoving = true;
    //                                 _plrGridPosition += _plrMoveDirection;
    //                             }
                                

    //                             return;
    //                         case "trap":
    //                             if (spot_obj.GetComponent<trap_state>().TrapEnabled)
    //                             {
    //                                 //death sequence based on trap type
    //                                 death_use = death_types[(int)spot_obj.GetComponent<trap_state>().Type];
    //                                 plr_frame = death_use.x;
    //                                 plr_death = true;
    //                                 AudioSource.PlayClipAtPoint(audioClips[3], player.GetComponent<Transform>().localPosition);
    //                             }
    //                             else
    //                             {
    //                                 _plrMoving = true;
    //                                 _plrGridPosition += _plrMoveDirection;
    //                             }
    //                             return;
    //                         case "Finish":
    //                             AudioSource.PlayClipAtPoint(audioClips[5], player.GetComponent<Transform>().localPosition);                                
    //                             _plrMoving = true;
    //                             _plrGridPosition += _plrMoveDirection;
    //                             StartCoroutine(fadeToNextScene(SceneManager.GetActiveScene().buildIndex + 1, Time.realtimeSinceStartup));

    //                             return;
    //                         default:
    //                             //print("unknown");
    //                             _plrMoving = true;
    //                             _plrGridPosition += _plrMoveDirection;
    //                             return;

    //                     }
    //                 }
                    
    //             }
    //         // }
    //         // else if (_plrMoving)
    //         // {
    //             plr_timer1 += 1;
    //             _plrPosition = UpdatePosition(player, _plrPosition + (Time.deltaTime * new Vector3(_plrMoveDirection.x, _plrMoveDirection.y, 0) * 2f));
    //             if (spot_obj != null)
    //             {
    //                 if (spot_obj.CompareTag("obstacle") && (_plrPosition - spot_obj.GetComponent<Transform>().localPosition).magnitude > 0.5 && (int) spot_obj.GetComponent<state_chg>().Type == 1)
    //                 {
    //                     spot_obj.GetComponent<state_chg>().Crossing = false;
    //                     GameObject[] objects = GameObject.FindGameObjectsWithTag("trap");
    //                     foreach (GameObject obj in objects)
    //                     {
    //                         if ((obj.GetComponent<trap_state>().Type == spot_obj.GetComponent<state_chg>().Type) && (!spot_obj.GetComponent<state_chg>().State))
    //                         {
    //                             obj.GetComponent<trap_state>().TrapEnabled = true;
    //                         }
    //                     }
    //                 }
    //                 else if (spot_obj.CompareTag("obstacle") && (_plrPosition - spot_obj.GetComponent<Transform>().localPosition).magnitude <= 0.5 && (int) spot_obj.GetComponent<state_chg>().Type == 1)
    //                 {
    //                     spot_obj.GetComponent<state_chg>().Crossing = true;
    //                     GameObject[] objects = GameObject.FindGameObjectsWithTag("trap");
    //                     foreach (GameObject obj in objects)
    //                     {
    //                         if ((obj.GetComponent<trap_state>().Type == spot_obj.GetComponent<state_chg>().Type) && (!spot_obj.GetComponent<state_chg>().State))
    //                         {
    //                             obj.GetComponent<trap_state>().TrapEnabled = false;
    //                         }
    //                     }
    //                 }
    //             }
    //             //print((_plrPosition - (_plrGridPosition + new Vector3(0.5f, 0.5f, 0))).magnitude);
    //             if ((_plrPosition - (_plrGridPosition + new Vector3(0.5f, 0.5f, 0))).magnitude <= 0.05)
    //             {
    //                 _plrPosition = UpdatePosition(player, _plrGridPosition + new Vector3(0.5f, 0.5f, 0));
    //                 plr_timer1 = 0;
    //                 _plrMoving = false;
    //             }
    //             else
    //             {

    //                 if (plr_timer1 > 15)
    //                 {
    //                     plr_timer1 = 0;
    //                     plr_frame += 1;
    //                     switch (plr_state)
    //                     {
    //                         case 1:
    //                             if (plr_frame >= plr_walksprites.Length)
    //                             {
    //                                 plr_frame = 0;
    //                             }
    //                             updateSprite(player, plr_walksprites[plr_frame], new Vector3(0, 0, _plrFacingDirection - 90));
    //                             return;
    //                         case 2:
    //                             if (plr_frame >= plr_bombsprites.Length)
    //                             {
    //                                 plr_frame = 0;
    //                             }
    //                             updateSprite(player, plr_bombsprites[plr_frame], new Vector3(0, 0, _plrFacingDirection - 90));
    //                             return;
    //                         case 3:
    //                             if (plr_frame >= (plr_torchsprites.Length - 18))
    //                             {
    //                                 plr_frame = 0;
    //                             }
    //                             updateSprite(player, plr_torchsprites[plr_frame], new Vector3(0, 0, _plrFacingDirection - 90));
    //                             return;
    //                     }
    //                 }
    //             }
    //         // }
    //         // else if (!_plrMoving)
    //         // {

    //         //     switch (plr_state)
    //         //     {
    //         //         case 1:
    //         //             updateSprite(player, plr_walksprites[1], new Vector3(0, 0, _plrFacingDirection - 90));
    //         //             return;
    //         //         case 2:
    //         //             updateSprite(player, plr_bombsprites[1], new Vector3(0, 0, _plrFacingDirection - 90));
    //         //             return;
    //         //         case 3:
    //         //             updateSprite(player, plr_torchsprites[1], new Vector3(0, 0, _plrFacingDirection - 90));
    //         //             return;
    //         //     }


    //         // };
    }

}
