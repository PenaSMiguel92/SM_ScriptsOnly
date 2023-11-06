using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animator : MonoBehaviour
{
    private Sprite[] sprites; //sprites to use.
    [SerializeField] private bool loop;
    private int curFrame;
    public int CurrentFrame {set { curFrame = value; }}
    private const int FRAMERATE = 2;

    private SpriteRenderer curRenderer;
    private Transform curTransform;
    public event EventHandler onAnimationEnd;

    public Sprite[] Sprites {set { sprites = value; }}
    // Start is called before the first frame update
    public void BeiginAnimation() {
        StartCoroutine(PlayAnimation(loop));
    }
    void UpdateSprite( Sprite sprite, Vector3 rotation)
    {
        curTransform.localEulerAngles = rotation;
        curRenderer.sprite = sprite;
        return;
    }

    IEnumerator PlayAnimation(bool _loop)
    {
        bool end_anim = false;
        const int FRAME_RATE = FRAMERATE; //how many frames per animation frame.
        int cur_frame = (curFrame > sprites.Length - 1) ? sprites.Length - 1 : curFrame;
        int timer1 = 0;
        while (!end_anim)
        {
            timer1 += 1;
            if (timer1 > FRAME_RATE)
            {
                timer1 = 0;


                cur_frame += 1;
                //cur_frame = cur_frame > (_plrCurrentSprites.Length - 1) ? _plrCurrentSprites.Length-1 : cur_frame;
                if (cur_frame > sprites.Length - 1)
                {
                    cur_frame = 0;
                    if (!_loop)
                    {
                        end_anim = true;
                    }
                }

            }
            curFrame = cur_frame;
            UpdateSprite( sprites[cur_frame], new Vector3(0, 0, 0));
            yield return new WaitForEndOfFrame();
        }
        onAnimationEnd?.Invoke(this, EventArgs.Empty);
    }

    void Start()
    {
        curRenderer = gameObject.GetComponent<SpriteRenderer>();
        curTransform = gameObject.GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
