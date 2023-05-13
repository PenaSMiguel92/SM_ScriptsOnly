using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum EnemyType { Guard, Gatherer, Ninja, Mutant, Clone }
public enum EnemyState { Loading, Idle, Walk, Attack, Follow, Death }

public interface IEnemyAI
{
    public Vector3 LocalPosition { get; }
    public void Kill(DeathType _deathType);
}
public class EnemyAI : MonoBehaviour, IEnemyAI
{
    [SerializeField] private EnemyType _enemyType;
    [SerializeField] private Sprite[] _enemyWalkSprites;
    [SerializeField] private Sprite[] _enemyStandardDeathSprites;
    [SerializeField] private Sprite[] _enemyElectricutionDeathSprites;
    [SerializeField] private Sprite[] _enemyBurnDeathSprites;
    private Sprite[] _enemyCurrentSprites;
    private int[] _enemyChancesBehaviour;
    private Transform _enemyTransform;
    private GameControl _mainControl;
    private Player _mainPlayer;
    private AudioManager _audioManager;
    private Vector3 _enemyPosition;
    public Vector3 LocalPosition {get { return _enemyTransform.localPosition; } }
    private Vector3Int _enemyGridPosition;
    private float _enemyRndDirection;
    private Vector2 _enemyInputDirection;
    private int _enemyFacingDirection;
    private Vector3Int _enemyMoveDirection;
    private float _enemyMoveT;
    private const int _ENEMYFRAMERATE = 20;
    private const float _ENEMYMOVESPEED = 2f;
    private int _enemyLastFrame;
    private IEnumerator _enemyAnimation;
    private EnemyState _enemyState = EnemyState.Loading;

    public event EventHandler OnEnemyAnimationEnd;



    void Start()
    {
        _mainControl = GameControl.Main;
        _mainControl.onGameStart += OnGameStart;
        _enemyTransform = gameObject.GetComponent<Transform>();
        _mainPlayer = Player.Main;

    }

    void OnGameStart(object _sender, EventArgs _e)
    {
        _enemyPosition = _enemyTransform.localPosition;
        _enemyGridPosition = _mainControl.GetGridPosition(_enemyPosition);
        _enemyCurrentSprites = _enemyWalkSprites;
        switch (_enemyType)
        {
            case EnemyType.Guard: //guard
                _enemyChancesBehaviour = new int[] { 0, 100, 0, 0, 0, 0, 0, 0, 0, 0 }; //lower and upper bounds for select rnd direction and walk(min,max), keep walking(min,max), attack at current location (min, max), follow player (min,max),  
                break;
            case EnemyType.Gatherer: //gatherer
                _enemyChancesBehaviour = new int[] { 0, 8, 9, 40, 41, 95, 96, 100 }; //lower and upper bounds for select rnd direction and walk(min,max), keep walking(min,max), attack at current location (min, max), follow player (min,max),  
                break;
            case EnemyType.Ninja: //ninja
                _enemyChancesBehaviour = new int[] { 0, 6, 7, 35, 36, 65, 66, 100 }; //lower and upper bounds for select rnd direction and walk(min,max), keep walking(min,max), attack at current location (min, max), follow player (min,max),  
                break;
        }
        _enemyState = EnemyState.Idle;
    }

    void Update()
    {
        if (_enemyState == EnemyState.Loading) return;
        if (_enemyState == EnemyState.Death) return;
        if (_mainPlayer.State == PlayerState.Death) return;
        EnemyAIMakeDecision();
        if ((_mainPlayer.GetPosition() - _enemyTransform.localPosition).magnitude < 0.707)
        {
            _mainPlayer.Kill(DeathType.Standard);
        }
    }
    public void Kill(DeathType _deathtype)
    {
        if (_deathtype == DeathType.Acid) return;
        _enemyState = EnemyState.Death;
        switch(_deathtype)
        {
            case DeathType.Standard:
                _enemyCurrentSprites = _enemyStandardDeathSprites;
                break;
            case DeathType.Electricution:
                _enemyCurrentSprites = _enemyElectricutionDeathSprites;
                break;
            case DeathType.Burn:
                _enemyCurrentSprites = _enemyBurnDeathSprites;
                break;
        }
        _enemyAnimation = (_enemyAnimation == null) ? PlayAnimation(false) : _enemyAnimation;
        StartCoroutine(_enemyAnimation);
    }
    int Direction2Deg(float y, float x)
    {
        float degrees = Mathf.Rad2Deg * Mathf.Atan2(y, x);
        int result = Mathf.RoundToInt(180f - degrees);

        return result;
    }

    Vector3 UpdatePosition( Vector3 _position)
    {
        _enemyTransform.localPosition = _position;
        return _enemyTransform.localPosition;
    }
    void UpdateSprite(Sprite sprite, Vector3 rotation)
    {
        _enemyTransform.localEulerAngles = rotation;
        SpriteRenderer sp_rend = gameObject.GetComponent<SpriteRenderer>();
        sp_rend.sprite = sprite;
    }

    void EnemyInitiateWalk()
    {
        _enemyCurrentSprites = _enemyWalkSprites;
        _enemyGridPosition += _enemyMoveDirection;
        _enemyAnimation = (_enemyAnimation == null) ? PlayAnimation(true) : _enemyAnimation;
        StartCoroutine(_enemyAnimation);
        _enemyMoveT = 0;
        _enemyState = EnemyState.Walk;
    }
    void UpdateEnemyMovementParameters(float _direction)
    {
        _enemyInputDirection = new Vector2(Mathf.Cos(_direction), Mathf.Sin(_direction));
        _enemyFacingDirection = Direction2Deg(-_enemyInputDirection.y, _enemyInputDirection.x);
        _enemyMoveDirection = new Vector3Int(Mathf.RoundToInt(Mathf.Cos(_direction)), Mathf.RoundToInt(Mathf.Sin(_direction)), 0);
    }
    bool TestDirection(float _direction) //in radians.
    {
        bool _proceed = false;
        Vector3Int tmp_enemyMoveDirection = new Vector3Int(Mathf.RoundToInt(Mathf.Cos(_direction)), Mathf.RoundToInt(Mathf.Sin(_direction)), 0);
        if (!_mainControl.HasTile(_enemyGridPosition + tmp_enemyMoveDirection)) return true;

        GameObject tmp_spot_obj = _mainControl.GetInstantiatedObject(_enemyGridPosition + tmp_enemyMoveDirection);
        if (tmp_spot_obj != null)
        {
            switch (tmp_spot_obj.tag)
            {
                case "wall":
                    _proceed = false;
                    break;
                case "push":
                    _proceed = false;
                    break;
                case "Finish":
                    _proceed = false;
                    break;
                case "door":
                    _proceed = false;
                    break;
                case "switch":
                    _proceed = tmp_spot_obj.GetComponent<TrapSwitchHandle>().TestTrapSwitch(gameObject, tmp_enemyMoveDirection);
                    break;
                case "trap":
                    var TrapStatus = tmp_spot_obj.GetComponent<TrapHandle>().TestTrap(tmp_enemyMoveDirection, gameObject);
                    _proceed = TrapStatus.Item3;
                    if (TrapStatus.Item2)
                    {
                        Kill(TrapStatus.Item1);
                    }
                    break;
                default:
                    _proceed = true;
                    break;
            }
        }
        return _proceed;
    }

    float SelectRandomDirection()
    {
        List<float> rndRadDirectionList = new List<float> { 0, 0.5f * Mathf.PI, Mathf.PI, 1.5f * Mathf.PI, 2f * Mathf.PI };
        int indexChosen = UnityEngine.Random.Range(0, rndRadDirectionList.Count);
        float rndDirection = rndRadDirectionList[indexChosen];
        return rndDirection;
    }

    IEnumerator FollowPlayer()
    {

        yield return new WaitForEndOfFrame();
    }

    IEnumerator PathFindTowardsPoint(GameObject obj, Vector3Int target)
    {
        yield return new WaitForEndOfFrame();
    }

    IEnumerator PlayAnimation(bool _loop) //frames is vect2d with x being start and y being end frames
    {
        bool end_anim = false;
        const int FRAME_RATE = _ENEMYFRAMERATE;
        int cur_frame = (_enemyLastFrame > _enemyCurrentSprites.Length - 1) ? 0 : _enemyLastFrame;
        int timer1 = 0;
        while (!end_anim)
        {
            timer1 += 1;
            if (timer1 > FRAME_RATE)
            {
                timer1 = 0;


                cur_frame += 1;
                if (cur_frame > _enemyCurrentSprites.Length - 1)
                {
                    cur_frame = 0;
                    if (!_loop)
                    {
                        end_anim = true;
                    }
                }

            }
            _enemyLastFrame = cur_frame;
            UpdateSprite( _enemyCurrentSprites[cur_frame], new Vector3(0, 0, _enemyFacingDirection + 90));
            yield return new WaitForEndOfFrame();
        }
        OnEnemyAnimationEnd?.Invoke(this, EventArgs.Empty);
    }

    void EnemyAIMakeDecision()
    {
        switch (_enemyState)
        {
            case EnemyState.Idle: //standing, make choice of motion type
                int rndChoice = Mathf.RoundToInt(UnityEngine.Random.value * 100);
                if (rndChoice > _enemyChancesBehaviour[0] && rndChoice <= _enemyChancesBehaviour[1]) //10% chance of choosing random direction.
                {
                    
                    if (!TestDirection(_enemyRndDirection))
                    {
                        _enemyRndDirection =  SelectRandomDirection();
                        break;
                    } 
                    
                    UpdateEnemyMovementParameters(_enemyRndDirection);
                    EnemyInitiateWalk(); //choose a direction, and then walk.
                }
                else if (rndChoice > _enemyChancesBehaviour[2] && rndChoice <= _enemyChancesBehaviour[3]) //walk along straight path until blocked.
                {
                    // if (!TestDirection(_enemyRndDirection))
                    // {
                    //     _enemyRndDirection = SelectRandomDirection();
                    //     break;
                    // }
                    // UpdateEnemyMovementParameters(_enemyRndDirection);
                    // EnemyInitiateWalk(); //keep walking unless blocked.
                }
                else if (rndChoice > _enemyChancesBehaviour[4] && rndChoice <= _enemyChancesBehaviour[5]) //chance of striking at current location if enemy_type other than guard and select a random direction afterwards. Chances change according to enemy type!
                {
                    //strike at current location w/ function and then commence walking.
                    //selectRandomDirection();
                    //ai_state = 2;

                }
                else if (rndChoice > _enemyChancesBehaviour[6] && rndChoice <= _enemyChancesBehaviour[7])
                {
                    //follow player
                }
                break;
            case EnemyState.Walk:
                _enemyMoveT = Mathf.Min(1, _enemyMoveT + Time.deltaTime * _ENEMYMOVESPEED);
                UpdatePosition(Vector3.Lerp(_enemyPosition, _enemyGridPosition + new Vector3(0.5f, 0.5f, 0), _enemyMoveT));
                if (_enemyMoveT == 1)
                {
                    _enemyPosition = UpdatePosition(_enemyGridPosition + new Vector3(0.5f, 0.5f, 0));
                    _enemyMoveT = 0;
                    if (_enemyAnimation != null)
                    {
                        StopCoroutine(_enemyAnimation);
                    }
                    _enemyState = EnemyState.Idle;
                    
                }
                break;
            case EnemyState.Attack: //striking
                break;
            case EnemyState.Follow: //following
                break;
            case EnemyState.Death:
                break;
        }

    }

    

    
}
