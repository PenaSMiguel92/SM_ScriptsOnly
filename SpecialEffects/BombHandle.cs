using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBomb
{
    public BombState State { get; }
    public float SpawnTime { set; }
}

public class BombHandle : MonoBehaviour, IBomb
{
    [SerializeField] Sprite[] _sprites;
    [SerializeField] GameObject _ExplosionObject;
    Player _mainPlayer;
    GameControl _mainControl;
    AudioManager _audioManager;
    SpriteRenderer _spriteRenderer;
    Transform _transform;
    BombState _state = BombState.Idle;

    float _spawnTime;
    bool _flashState = false;
    int _flashStateInt = 0;
    float _flashFrame = 0;
    float _flashInterval = 60f;
    const float _maxFlashInterval = 60f;
    const float _flashDuration = 4f;
    float _flashT = 0;

    float _moveT = 0;
    float _moveSpeed = 0;
    Vector3 _gridOffset = new Vector3(0.5f, 0.5f, 0);
    Vector3 _position;
    Vector3Int _gridPosition;

    public BombState State {get { return _state; } }
    public float SpawnTime {set { _spawnTime = value; } }
    // Start is called before the first frame update
    void Start()
    {
        _mainControl = GameControl.Main;
        _mainPlayer = Player.Main;
        _audioManager = AudioManager.Main;
        _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        _transform = gameObject.GetComponent<Transform>();
        StartCoroutine(FlashingAnimation());
        _position = _transform.localPosition;
        _gridPosition = _mainControl.GetGridPosition(_transform.localPosition);
    }
    void OnExplosionAnimationEnd(object _sender, EventArgs _e)
    {
        Destroy(gameObject);
    }
    GameObject SpawnExplosionAtLocation(Vector3Int _location, bool _center)
    {
        List<TilemapUse> tmp_tilemaps = new List<TilemapUse> { TilemapUse.Foreground, TilemapUse.Moveables };
        foreach (TilemapUse tmp_tilemap in tmp_tilemaps)
        {
            
            
            if (!_mainControl.HasTile(_location, tmp_tilemap)) continue;
            GameObject tmp_spot_obj = _mainControl.GetInstantiatedObject(_location, tmp_tilemap);
            if (tmp_spot_obj == null) continue;
            switch(tmp_spot_obj.tag)
            {
                case "bomb_destructible":
                    _mainControl.SetTile(_location, null, tmp_tilemap);
                    break;
                default:
                    if (_center) break;
                    return null;
            }
        }
        return Instantiate(_ExplosionObject, _location + _gridOffset, Quaternion.Euler(0, 0, 0));
    }
    void SpawnExplosions()
    {
        float[] tmp_angles = { 0, 0.5f * Mathf.PI, Mathf.PI, 1.5f * Mathf.PI };
        Vector3 tmp_newLocation;
        Vector3Int tmp_newGridLocation;
        foreach (float tmp_angle in tmp_angles)
        {
            Vector3 tmp_delta = new Vector3(Mathf.Cos(tmp_angle), Mathf.Sin(tmp_angle), 0);
            tmp_newLocation = _transform.localPosition + tmp_delta;
            tmp_newGridLocation = _mainControl.GetGridPosition(tmp_newLocation);

            SpawnExplosionAtLocation(tmp_newGridLocation, false);
        }

        tmp_newGridLocation = _mainControl.GetGridPosition(_transform.localPosition);
        GameObject tmp_explosion = SpawnExplosionAtLocation(tmp_newGridLocation, true);
        tmp_explosion.GetComponent<ExplosionHandle>().onAnimationEnd += OnExplosionAnimationEnd;
        _spriteRenderer.forceRenderingOff = true;
        _audioManager.PlaySound(SoundType.Explosion, _transform.localPosition);
        _state = BombState.Exploded;
    }
    IEnumerator FlashingAnimation()
    {
        bool _endAnim = false;
        while (!_endAnim)
        {
            _flashT = Mathf.Min(1, (Time.realtimeSinceStartup - _spawnTime) / _flashDuration);
            _flashInterval = Mathf.Lerp(_maxFlashInterval, 1, _flashT);
            _flashFrame += 1;
            if (_flashFrame > _flashInterval)
            {
                _flashFrame = 0;
                _flashState = !_flashState;
                _flashStateInt = _flashState ? 1 : 0;
                _spriteRenderer.sprite = _sprites[_flashStateInt];
            }
            if (_flashT == 1)
            {
                if (_state == BombState.Moving)
                {
                    _state = BombState.Waiting;
                    break;
                }
                SpawnExplosions();
                _state = BombState.Exploding;

                break;
            }
            yield return new WaitForEndOfFrame();
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
                case "switch":
                    _proceed = tmp_spot_obj.GetComponent<TrapSwitchHandle>().TestTrapSwitch(gameObject, _moveDirection);
                    break;
                case "trap":
                    //ask object whether it is currently active, what type of death to initiate, and then when or not player can continue moving.
                    var TrapStatus = tmp_spot_obj.GetComponent<TrapHandle>().TestTrap(_moveDirection, gameObject);
                    _proceed = TrapStatus.Item3;      
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
            _gridPosition += _moveDirection;
            _moveSpeed = _speed;
            _state = BombState.Moving;
        }
    }

    public void ForceMove(Vector3Int _direction, float _speed)
    {
        if (_state == BombState.Exploded) return;
        if (_state == BombState.Exploding) return;
        if (_state != BombState.Idle) return;
        Vector3Int tmp_testingLocation = _gridPosition + _direction;
        TestNextForcedLocation(tmp_testingLocation, _direction, _speed);
    }
    Vector3 UpdatePosition(Vector3 tmp_position)
    {
        _transform.localPosition = tmp_position;
        return _transform.localPosition;
    }
    void LerpMove(bool _waiting)
    {
        _moveT = Mathf.Min(1, _moveT + (Time.deltaTime*_moveSpeed));
        UpdatePosition(Vector3.Lerp(_position, _gridPosition + _gridOffset, _moveT)); 
        if (_moveT == 1)
        {
            _moveT = 0f;
            _position = UpdatePosition( _gridPosition + _gridOffset);
            _state = BombState.Idle;
            if (_waiting)
            {
                SpawnExplosions();
                _state = BombState.Exploding;
            }
        }
    }
    void CheckForConveyor()
    {
        if (!_mainControl.HasTile(_gridPosition,TilemapUse.Foreground)) return;
        GameObject tmp_spot_obj = _mainControl.GetInstantiatedObject(_gridPosition, TilemapUse.Foreground);
        if (tmp_spot_obj == null) return;
        if (tmp_spot_obj.tag == "mover")
        {
            Conv_Directional tmp_convDir = tmp_spot_obj.GetComponent<Conv_Directional>();
            ForceMove(tmp_convDir.Direction, tmp_convDir.Speed);
        }
    }
    void PerformAction()
    {
        if (_state == BombState.Exploding) return;
        if (_state == BombState.Exploded) return;
        switch(_state)
        {
            case BombState.Idle:
                CheckForConveyor();
                break;
            case BombState.Waiting:
                LerpMove(true);
                break;
            case BombState.Moving:
                LerpMove(false);
                break;
        }
    }
    void Update()
    {
        PerformAction();
    }
}
