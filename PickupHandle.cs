using System;
using UnityEngine;

public enum PickUpEnum { Coin, CoinChest, RedKey, BlueKey, YellowKey, GreenKey, Bomb, Torch }
public enum PickUpState {Loading, Idle, PickedUp}
public interface IPickupType
{
    public PickUpEnum TypeOfPickup { get; }
    public PickUpState State { get; }
    public string Name { get; }
}
public class PickupHandle : MonoBehaviour, IPickupType
{

    [SerializeField] private PickUpEnum _typeOfPickup;
    private string _nameOfObject;
    private string[] names = { "coin", "coin_chest", "key_pickup_0", "key_pickup_1", "key_pickup_2", "key_pickup_3", "bomb" };

    public PickUpEnum TypeOfPickup { get { return _typeOfPickup; } }

    public string Name { get { return _nameOfObject; } }
    private PickUpState _state = PickUpState.Loading;
    public PickUpState State {get { return _state; } }
    private Transform _pickupTransform;

    private GameControl _mainControl;
    private Player _mainPlayer;

    void Start()
    {
        _mainControl = GameControl.Main;
        _mainPlayer = Player.Main;
        _mainControl.onGameStart += OnGameStart;
        _pickupTransform = gameObject.GetComponent<Transform>();
    }

    void OnGameStart(object _sender, EventArgs _e)
    {
        _nameOfObject = names[(int)_typeOfPickup];
        _state = PickUpState.Idle;
    }

    void Update()
    {
        if (_state == PickUpState.Loading) return;
        if (_state == PickUpState.PickedUp) Destroy(gameObject);
        if ((_mainPlayer.GetPosition() - _pickupTransform.localPosition).magnitude < 0.707)
        {
            _mainPlayer.AddToInventory(_typeOfPickup);
            _state = PickUpState.PickedUp;
        }
    }

    // public void PickupItem(GameObject item)
    // {
    //     switch (item.GetComponent<pickup_type>().TypeOfPickup)
    //     {
    //         case PickUpEnum.Coin:
    //             //print("coin");
    //             AudioSource.PlayClipAtPoint(audioClips[0], item.GetComponent<Transform>().localPosition);
    //             plr_score += 100;
    //             foreground.GetComponent<Tilemap>().SetTile(plr_gridpos, null);

    //             return;
    //         case PickUpEnum.CoinChest:
    //             //print("coin_chest");
    //             AudioSource.PlayClipAtPoint(audioClips[1], item.GetComponent<Transform>().localPosition);
    //             plr_score += 500;
    //             foreground.GetComponent<Tilemap>().SetTile(plr_gridpos, null);
    //             return;
    //         default:
    //             AudioSource.PlayClipAtPoint(audioClips[0], item.GetComponent<Transform>().localPosition);
    //             addToInventory(item);
    //             foreground.GetComponent<Tilemap>().SetTile(plr_gridpos, null);
    //             return;

    //     }
    // }

}
