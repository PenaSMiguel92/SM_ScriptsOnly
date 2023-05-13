using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DoorType {CoinDoor, RedDoor, BlueDoor, GreenDoor, YellowDoor}
public enum DoorState {Loading, Idle, Opening}
public interface IDoor
{

    public bool TestDoor(List<PickUpEnum> _plrInventory);
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
    private GameControl _mainControl;
    private AudioManager _audioManager;
    
    private int _curFrame;
    public event EventHandler onAnimationEnd;

    // Start is called before the first frame update
    void Start()
    {
        _mainControl = GameControl.Main;
        _audioManager = AudioManager.Main;
        _mainControl.onGameStart += OnGameStart;
        onAnimationEnd += OnAnimationEnd;
        _curTransform = gameObject.GetComponent<Transform>();
        if (_mainControl.State == GameState.LevelPlay)
        {
            OnGameStart(this, EventArgs.Empty);
        }
    }
    void OnGameStart(object _sender, EventArgs _e)
    {
        _state = DoorState.Idle;
        gameObject.GetComponent<SpriteRenderer>().sprite = _sprites[0];
        _tileLoc = _mainControl.GetGridPosition(_curTransform.localPosition);
    }
    void OnAnimationEnd(object _sender, EventArgs _e)
    {
        _mainControl.SetTile(_tileLoc, null);
    }
    void UpdateSprite( Sprite sprite, Vector3 rotation)
    {
        _curTransform.localEulerAngles = rotation;
        SpriteRenderer sp_rend = gameObject.GetComponent<SpriteRenderer>();
        sp_rend.sprite = sprite;
        return;
    }
    IEnumerator PlayAnimation(bool _loop)
    {
        bool end_anim = false;
        const int FRAME_RATE = 20; //how many frames per animation frame.
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
    void DetermineCoinDoorState()
    {
        int coin_sum = 0;
        GameObject[] coins = GameObject.FindGameObjectsWithTag("pickup");
        foreach (GameObject coin in coins)
        {
            if ( coin.GetComponent<PickupHandle>().TypeOfPickup == PickUpEnum.Coin)
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
    public bool TestDoor(List<PickUpEnum> _plrInventory)
    {
        switch (_type)
        {
            // case TrapType.ElectricalBox:
            //     break;
            case DoorType.BlueDoor:
                if (_plrInventory.Contains(PickUpEnum.BlueKey))
                {
                    OpenDoor();
                    return true;
                }
                break;
            case DoorType.RedDoor:
                if (_plrInventory.Contains(PickUpEnum.RedKey))
                {
                    OpenDoor();
                    return true;
                }
                break;
            case DoorType.GreenDoor:
                if (_plrInventory.Contains(PickUpEnum.GreenKey))
                {
                    OpenDoor();
                    return true;
                }
                break;
            case DoorType.YellowDoor:
                if (_plrInventory.Contains(PickUpEnum.YellowKey))
                {
                    OpenDoor();
                    return true;
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
    }
    
}
