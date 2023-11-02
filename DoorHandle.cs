using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DoorType {CoinDoor, RedDoor, BlueDoor, GreenDoor, YellowDoor, Breakable}
public enum DoorState {Loading, Idle, Opening}
public interface IDoor
{

    public bool TestDoor(List<PickUpType> _plrInventory);
}

public class DoorHandle : MonoBehaviour, IStateChange<DoorType, DoorState>
{
    [SerializeField] private Sprite[] _sprites; //sprites to use.
    [SerializeField] private DoorType _type;
    public DoorType Type {get { return _type; } }
    private DoorState _state = DoorState.Loading;
    public DoorState State {get { return _state; } }

    private Vector3Int _tileLoc;
    private Transform _curTransform;
    private SpriteRenderer _curRenderer;
    private GameControl _mainControl;
    private AudioManager _audioManager;
    
    private int _curFrame;
    private const int _FRAMERATE = 2;
    public event EventHandler onAnimationEnd;

    private int bw_currentStep = 0; //bw = breakable wall variables
    private bool bw_timerSet = false;
    private float bw_timerValue;
    // Start is called before the first frame update
    void Start()
    {
        _mainControl = GameControl.Main;
        _audioManager = AudioManager.Main;
        _mainControl.onGameStart += OnGameStart;
        onAnimationEnd += OnAnimationEnd;
        _curRenderer = gameObject.GetComponent<SpriteRenderer>();
        _curTransform = gameObject.GetComponent<Transform>();
        if (_mainControl.State == GameState.LevelPlay)
        {
            OnGameStart(this, EventArgs.Empty);
        }
    }
    void OnGameStart(object _sender, EventArgs _e)
    {
        _state = DoorState.Idle;
        _curRenderer.sprite = _sprites[0];
        _tileLoc = _mainControl.GetGridPosition(_curTransform.localPosition);
    }
    void OnAnimationEnd(object _sender, EventArgs _e)
    {
        _mainControl.SetTile(_tileLoc, null, TilemapUse.Foreground);
    }
    void UpdateSprite( Sprite sprite, Vector3 rotation)
    {
        _curTransform.localEulerAngles = rotation;
        _curRenderer.sprite = sprite;
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
                        end_anim = true;
                    }
                }

            }
            _curFrame = cur_frame;
            UpdateSprite( _sprites[cur_frame], new Vector3(0, 0, 0));
            yield return new WaitForEndOfFrame();
        }
        onAnimationEnd?.Invoke(this, EventArgs.Empty);
    }
    void HandleBreakable()
    {
        if (bw_timerSet) return;
        bw_timerSet = true;
        bw_timerValue = Time.realtimeSinceStartup;
        bw_currentStep += 1;
        _curRenderer.sprite = _sprites[bw_currentStep];
    }
    void UpdateBreakableTimerState()
    {
        if (!bw_timerSet) return;
        float tmp_maxTime = 0.25f;
        if (Time.realtimeSinceStartup - bw_timerValue > tmp_maxTime)
        {
            bw_timerSet = false;
        }
    }
    void DetermineCoinDoorState()
    {
        int coin_sum = 0;
        GameObject[] coins = GameObject.FindGameObjectsWithTag("pickup");
        foreach (GameObject coin in coins)
        {
            if ( coin.GetComponent<PickupHandle>().TypeOfPickup == PickUpType.Coin)
            {
                coin_sum += 1;
            }
        }
        if (coin_sum <= 0)
        {
            OpenDoor();
        }
    }
    public void OpenDoor()
    {
        _state = DoorState.Opening;
        _audioManager.PlaySound(SoundType.DoorOpen, _curTransform.localPosition);
        StartCoroutine(PlayAnimation(false));
    }
    public bool TestDoor(List<PickUpType> _plrInventory)
    {
        if (_state == DoorState.Opening) return false;
        switch (_type)
        {
            // case TrapType.ElectricalBox:
            //     break;
            case DoorType.Breakable:
                if (bw_timerSet) return false;
                HandleBreakable();
                if (bw_currentStep > 4)
                {
                    _curFrame = bw_currentStep;
                    OpenDoor();
                }
                break;
            case DoorType.BlueDoor:
                if (_plrInventory.Contains(PickUpType.BlueKey))
                {
                    OpenDoor();
                }
                break;
            case DoorType.RedDoor:
                if (_plrInventory.Contains(PickUpType.RedKey))
                {
                    OpenDoor();
                }
                break;
            case DoorType.GreenDoor:
                if (_plrInventory.Contains(PickUpType.GreenKey))
                {
                    OpenDoor();
                }
                break;
            case DoorType.YellowDoor:
                if (_plrInventory.Contains(PickUpType.YellowKey))
                {
                    OpenDoor();
                }
                break;
            
        }
        return false;
    }
    void Update()
    {
        if (_state == DoorState.Loading) return;
        if (_state == DoorState.Opening) return;
        if (_state == DoorState.Idle && _type == DoorType.CoinDoor)
        {
            DetermineCoinDoorState();
        }
        if (_state == DoorState.Idle && _type == DoorType.Breakable)
        {
            UpdateBreakableTimerState();
        }
    }
    
}
