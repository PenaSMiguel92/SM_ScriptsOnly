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
    public EnemyState State { get; }
    public void Kill(DeathType _deathType);
}
public class enemy_ai : MonoBehaviour, IEnemyAI
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
    private Vector2 _enemyInputDirection;
    private int _enemyFacingDirection;
    private Vector3Int _enemyMoveDirection;
    private bool _enemyMoving;
    private int _enemyTimer1;
    private int _enemyLastFrame;
    private IEnumerator _enemyAnimation;
    private Coroutine _enemyAnimationPlayback;
    private EnemyState _enemyState = EnemyState.Loading;
    public EnemyState State {get { return _enemyState; } }

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
                _enemyChancesBehaviour = new int[] { 0, 10, 11, 100, 0, 0, 0, 0, 0, 0 }; //lower and upper bounds for select rnd direction and walk(min,max), keep walking(min,max), attack at current location (min, max), follow player (min,max),  
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
        if ((_mainPlayer.LocalPosition - _enemyTransform.localPosition).magnitude < 0.707)
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

    void EnemyAIMakeDecision()
    {
        switch (_enemyState)
        {
            case EnemyState.Idle: //standing, make choice of motion type
                int rndChoice = Mathf.RoundToInt(UnityEngine.Random.value * 100);
                if (rndChoice > _enemyChancesBehaviour[0] && rndChoice <= _enemyChancesBehaviour[1]) //80% chance of choosing random direction.
                {
                    SelectRandomDirection();
                    _enemyCurrentSprites = _enemyWalkSprites;
                    _enemyState = EnemyState.Walk; //walk along selected direction
                }
                else if (rndChoice > _enemyChancesBehaviour[2] && rndChoice <= _enemyChancesBehaviour[3]) //20% chance of striking at current location if enemy_type other than guard and select a random direction afterwards. Chances change according to enemy type!
                {
                    _enemyCurrentSprites = _enemyWalkSprites;
                    _enemyState = EnemyState.Walk; //keep walking
                }
                else if (rndChoice > _enemyChancesBehaviour[4] && rndChoice <= _enemyChancesBehaviour[5])
                {
                    //strike at current location w/ function and then commence walking.
                    //selectRandomDirection();
                    //ai_state = 2;

                }
                else if (rndChoice > _enemyChancesBehaviour[6] && rndChoice <= _enemyChancesBehaviour[7])
                {
                    //follow player
                }
                //anything other than guard can occasionally follow player!
                return;
            case EnemyState.Walk: //walking
                //print("walking");
                if (!_enemyMoving)
                {
                    _enemyMoving = true;
                    _enemyGridPosition += _enemyMoveDirection;
                    //print("Coroutine started");
                    _enemyAnimation = (_enemyAnimation == null) ? PlayAnimation(true) : _enemyAnimation;
                    //walkAnimation = StartCoroutine();
                    StartCoroutine(_enemyAnimation);
                    //_enemyAnimationPlayback = (_enemyAnimationPlayback == null) ? StartCoroutine(_enemyAnimation) : _enemyAnimationPlayback;
                }
                else
                {
                    if ((_enemyPosition - (_enemyGridPosition + new Vector3(0.5f, 0.5f, 0))).magnitude <= 0.05)
                    {
                        _enemyPosition = UpdatePosition(_enemyGridPosition + new Vector3(0.5f, 0.5f, 0));
                        //plr_timer1 = 0;
                        _enemyMoving = false;
                        if (_enemyAnimation != null)
                        {
                             StopCoroutine(_enemyAnimation);
                        }
                       
                        _enemyState = EnemyState.Idle;
                    }
                    else
                    {
                        GameObject spot_obj = _mainControl.GetInstantiatedObject(_enemyGridPosition);

                        if (spot_obj != null)
                        {
                            if (spot_obj.tag == "obstacle")
                            {
                                if (spot_obj.GetComponent<state_chg>().Crossable)
                                {
                                    _enemyPosition = UpdatePosition(_enemyPosition + (Time.deltaTime * new Vector3(_enemyInputDirection.x, _enemyInputDirection.y, 0) * 2f));
                                    return;
                                }
                            }
                            else if (spot_obj.tag == "pickup" || spot_obj.tag == "enemy" || spot_obj.tag == "Player")
                            {
                                _enemyPosition = UpdatePosition( _enemyPosition + (Time.deltaTime * new Vector3(_enemyInputDirection.x, _enemyInputDirection.y, 0) * 2f));
                                return;
                            }

                            _enemyGridPosition -= _enemyMoveDirection;
                            _enemyPosition = UpdatePosition( _enemyGridPosition + new Vector3(0.5f, 0.5f, 0));
                            _enemyMoving = false;
                            _enemyState = EnemyState.Idle;
                            //StopCoroutine(_enemyAnimation);



                        }
                        else
                        {
                            _enemyPosition = UpdatePosition( _enemyPosition + (Time.deltaTime * new Vector3(_enemyInputDirection.x, _enemyInputDirection.y, 0) * 2f));
                        }

                    }




                }
                return;
            case EnemyState.Attack: //striking
                return;
            case EnemyState.Follow: //following
                return;
            case EnemyState.Death:
                return;
        }

    }

    IEnumerator PathFindTowardsPoint(GameObject obj, Vector3Int target)
    {
        yield return new WaitForEndOfFrame();
    }

    IEnumerator PlayAnimation(bool _loop) //frames is vect2d with x being start and y being end frames
    {
        bool end_anim = false;
        const int FRAME_RATE = 20;
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

    void SelectRandomDirection()
    {
        List<float> rndRadDirectionList = new List<float> { 0, 0.5f * Mathf.PI, Mathf.PI, 1.5f * Mathf.PI, 2f * Mathf.PI };
        float rndRadDirection;
        int indexChosen;
        bool dirObtained = false;
        while (!dirObtained)
        {
            indexChosen = UnityEngine.Random.Range(0, rndRadDirectionList.Count);
            rndRadDirection = rndRadDirectionList[indexChosen];//(Mathf.Round(Random.Range(0, 4)) / 4f) * (2 * Mathf.PI);
            _enemyInputDirection = new Vector2(Mathf.Cos(rndRadDirection), -Mathf.Sin(rndRadDirection));
            _enemyFacingDirection = Direction2Deg(-_enemyInputDirection.y, _enemyInputDirection.x);
            _enemyMoveDirection = new Vector3Int(Mathf.RoundToInt(Mathf.Cos(rndRadDirection)), -Mathf.RoundToInt(Mathf.Sin(rndRadDirection)), 0);
            GameObject tmp_spot_obj = _mainControl.GetInstantiatedObject(_enemyGridPosition + _enemyMoveDirection);
            if (tmp_spot_obj != null)
            {
                if (tmp_spot_obj.tag == "obstacle")
                {
                    if (tmp_spot_obj.GetComponent<state_chg>().Crossable)
                    {
                        dirObtained = true;
                    }
                    else
                    {
                        rndRadDirectionList.RemoveAt(indexChosen);
                    }
                }
                else if (tmp_spot_obj.tag == "pickup" || tmp_spot_obj.tag == "enemy" || tmp_spot_obj.tag == "Player")
                {
                    dirObtained = true;
                }
                else
                {
                    rndRadDirectionList.RemoveAt(indexChosen);
                }
            }
            else
            {
                dirObtained = true;
            }
        }

        return;
    }

    IEnumerator FollowPlayer()
    {

        yield return new WaitForEndOfFrame();
    }
}
