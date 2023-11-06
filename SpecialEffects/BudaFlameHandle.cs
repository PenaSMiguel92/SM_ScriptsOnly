using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public interface IBudaFlame
{
    public FlameSection Section { get; set; }
}


public class BudaFlameHandle : MonoBehaviour, IBudaFlame
{
    [SerializeField] Sprite[] _spritesMidSection;
    [SerializeField] Sprite[] _spritesEndSection;
    Sprite[] _sprites; //current sprites being used.
    GameControl _mainControl;
    Transform _curTransform;
    Vector3Int _gridPos;
    FlameSection _section;
    SpriteRenderer _curSpriteRenderer;
    int _curFrame;
    const int _FRAMERATE = 2;
    public FlameSection Section {get { return _section; } set { _section = value; } }
    BudaFlameHandle(FlameSection _currentFlameSection)
    {
        _section = _currentFlameSection;
    }

    void Start()
    {
        _mainControl = GameControl.Main;
        _curTransform = gameObject.GetComponent<Transform>();
        _curSpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        _gridPos = _mainControl.GetGridPosition(_curTransform.localPosition);
        switch(_section)
        {
            case FlameSection.Mid:
                _sprites = _spritesMidSection;
                break;
            case FlameSection.End:
                _sprites = _spritesEndSection;
                break;
        }
        StartCoroutine(PlayAnimation(true));
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
        //onAnimationEnd?.Invoke(this, EventArgs.Empty);
    }
}
