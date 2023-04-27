using System;
using UnityEngine;

public enum PickUpEnum {Coin, CoinChest, RedKey, BlueKey, YellowKey, GreenKey}
public interface IPickupType 
{
    public PickUpEnum TypeOfPickup { get; }
    public string Name { get; }
}
public class pickup_type : MonoBehaviour, IPickupType
{
    
    [SerializeField] private PickUpEnum _typeOfPickup;
    private string _nameOfObject;
    private string[] names = { "coin", "coin_chest", "key_pickup_0", "key_pickup_1" , "key_pickup_2" , "key_pickup_3" };

    public PickUpEnum TypeOfPickup
    {
        get { return _typeOfPickup; }
    }

    public string Name 
    {
        get { return _nameOfObject; }
    }

    void Awake()
    {
        _nameOfObject = names[(int)_typeOfPickup];
    }

}
