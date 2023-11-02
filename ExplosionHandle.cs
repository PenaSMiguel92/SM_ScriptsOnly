using System;
using System.Collections;
using UnityEngine;

public class ExplosionHandle : MonoBehaviour
{
    [SerializeField] private Sprite[] _sprites;
    private ExplosionState _state = ExplosionState.Searching;
    private Player _mainPlayer;
    private Transform _curTransform;
    private SpriteRenderer _curSpriteRenderer;
    private int _curFrame;
    private const int _FRAMERATE = 2;
    public event EventHandler onAnimationEnd;
    // Start is called before the first frame update
    void Start()
    {
        _mainPlayer = Player.Main;
        _curTransform = gameObject.GetComponent<Transform>();
        _curSpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        onAnimationEnd += OnAnimationEnd;
        StartCoroutine(PlayAnimation(false));
    }
    void OnAnimationEnd(object _sender, EventArgs _e)
    {
        _state = ExplosionState.Done;
        Destroy(gameObject);
    }
    void UpdateSprite( Sprite sprite, Vector3 rotation)
    {
        _curTransform.localEulerAngles = rotation;
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
        onAnimationEnd?.Invoke(this, EventArgs.Empty);
    }

    void Update()
    {
        if (_state == ExplosionState.Done) return;
        if ((_mainPlayer.GetPosition() - _curTransform.localPosition).magnitude <= 0.707)
        {
            _mainPlayer.Kill(DeathType.Standard);
            _state = ExplosionState.Done;
        }
        IEnemyAI[] _enemies = GetComponents<IEnemyAI>();
        foreach (IEnemyAI _enemy in _enemies)
        {
            if ((_enemy.LocalPosition - _curTransform.localPosition).magnitude <= 0.707)
            {
                _enemy.Kill(DeathType.Standard);
                _state = ExplosionState.Done;
                break;
            }
        }
    }
}
