using System;
using System.Collections.Generic;
using UnityEngine;

public enum BombState { Waiting, Exploding, Exploded}
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
    BombState _state = BombState.Waiting;

    float _spawnTime;
    bool _flashState = false;
    int _flashStateInt = 0;
    float _flashFrame = 0;
    float _flashInterval = 50;
    const float _maxFlashInterval = 50;
    const float _flashDuration = 3;
    float _flashT = 0;
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
        return Instantiate(_ExplosionObject, _location + new Vector3(0.5f, 0.5f, 0), Quaternion.Euler(0, 0, 0));
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
    }
    void PerformAction()
    {
        if (_state == BombState.Exploded) return;
        switch(_state)
        {
            case BombState.Waiting:
                _flashT = Mathf.Min(1, (Time.realtimeSinceStartup - _spawnTime) / _flashDuration);
                _flashInterval = Mathf.Lerp(_maxFlashInterval, 5, _flashT);
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
                    _state = BombState.Exploding;
                }
                break;
            case BombState.Exploding:
                SpawnExplosions();
                _state = BombState.Exploded;
                break;
        }
    }
    void Update()
    {
        PerformAction();
    }
}
