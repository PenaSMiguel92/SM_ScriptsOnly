using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ConvDir {Up, Down, Left, Right}
public enum ConvState {Loading, Idle}
public interface IConveyor
{
    public Vector3Int Direction { get; }
    public float Speed { get; }
}

public class Conv_Directional : MonoBehaviour, IConveyor
{
    [SerializeField] private ConvDir _direction;
    [SerializeField] private Sprite[] _sprites;
    [SerializeField] private float _moveSpeed = 6f;
    public Vector3Int Direction { get { return _moveDirection; } }
    public float Speed {get { return _moveSpeed; } }
    private ConvState _state = ConvState.Loading;
    private List<Vector3Int> _moveVectors = new List<Vector3Int> { new Vector3Int(0, 1, 0), new Vector3Int(0, -1, 0), new Vector3Int(-1, 0, 0), new Vector3Int(1, 0, 0) };
    private Vector3Int _moveDirection = new Vector3Int();

    private GameControl _mainControl;
    private Player _mainPlayer;
    private Transform _curTransform;
    private Vector3Int _curGridPosition;
    private SpriteRenderer _curRenderer;
    private int _curFrame = 0;
    private const int _FRAMERATE = 2;
    
    void Start()
    {
        _mainControl = GameControl.Main;
        _mainPlayer = Player.Main;
        _mainControl.onGameStart += OnGameStart;
        _curTransform = gameObject.GetComponent<Transform>();
        _curRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

    void OnGameStart(object _sender, EventArgs _e)
    {
        StartCoroutine(PlayAnimation(true));
        _curGridPosition = _mainControl.GetGridPosition(_curTransform.localPosition);
        _moveDirection = _moveVectors[(int)_direction];
        _state = ConvState.Idle;
    }
    void UpdateSprite( Sprite sprite, Vector3 rotation)
    {
        _curTransform.localEulerAngles = rotation;
        _curRenderer.sprite = sprite;
        return;
    }

    void TestForMoveables()
    {
        if ((_mainPlayer.GetPosition() - _curTransform.localPosition).magnitude < 0.707 && _mainPlayer.State == PlayerState.Idle)
        {
            _mainPlayer.ForceMove(_moveDirection, _moveSpeed);
            return;
        }
        if (_mainControl.HasTile(_curGridPosition, TilemapUse.Moveables))
        {
            GameObject tmp_spotObj = _mainControl.GetInstantiatedObject(_curGridPosition, TilemapUse.Moveables);
            if (tmp_spotObj == null) return;
            IPushable tmp_pushableHandle = tmp_spotObj.GetComponent<IPushable>();
            if (tmp_pushableHandle.State != PushState.Idle) return;
            tmp_pushableHandle.ForcePush(_moveDirection, _moveSpeed);
        }
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
                        end_anim = true;
                    }
                }

            }
            _curFrame = cur_frame;
            UpdateSprite( _sprites[cur_frame], new Vector3(0, 0, 0));
            yield return new WaitForEndOfFrame();
        }
    }


    void Update()
    {
        if (_state == ConvState.Loading) return;
        TestForMoveables();
    }
}
