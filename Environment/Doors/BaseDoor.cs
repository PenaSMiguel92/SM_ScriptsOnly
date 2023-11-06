using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseDoor : MonoBehaviour, IStateChange<DoorType, DoorState>
{
    [SerializeField] protected Sprite[] sprites; //sprites to use.
    [SerializeField] protected DoorType type;
    public DoorType Type {get { return type; } }
    protected DoorState state = DoorState.Loading;
    public DoorState State {get { return state; } }

    protected Vector3Int tileLoc;
    protected Transform curTransform;
    private SpriteRenderer curRenderer;
    protected Animator animator;
    protected GameControl mainControl;
    protected AudioManager audioManager;
    void OnGameStart(object _sender, EventArgs _e)
    {
        state = DoorState.Idle;
        curRenderer.sprite = sprites[0];
        tileLoc = mainControl.GetGridPosition(curTransform.localPosition);
    }
    void OnAnimationEnd(object _sender, EventArgs _e)
    {
        mainControl.SetTile(tileLoc, null, TilemapUse.Foreground);
    }
    public void OpenDoor()
    {
        state = DoorState.Opening;
        audioManager.PlaySound(SoundType.DoorOpen, curTransform.localPosition);
        animator.BeiginAnimation();
    }
    void Start()
    {
        mainControl = GameControl.Main;
        mainControl.onGameStart += OnGameStart;
        audioManager = AudioManager.Main;
        animator = gameObject.GetComponent<Animator>();
        animator.Sprites = sprites;
        animator.onAnimationEnd += OnAnimationEnd;
        curRenderer = gameObject.GetComponent<SpriteRenderer>();
        curTransform = gameObject.GetComponent<Transform>();
        if (mainControl.State == GameState.LevelPlay)
        {
            OnGameStart(this, EventArgs.Empty);
        }
    }
}
