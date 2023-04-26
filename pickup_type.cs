using System;
using UnityEngine;

public class pickup_type : MonoBehaviour
{
    
    public int typeofPickup;
    private string nameofobject;
    private string[] types = { "coin", "coin_chest", "key_pickup_0", "key_pickup_1" , "key_pickup_2" , "key_pickup_3" };
    // Start is called before the first frame update


    void Start()
    {
        nameofobject = gameObject.GetComponent<SpriteRenderer>().sprite.name;
        typeofPickup = System.Array.BinarySearch<string>(types, nameofobject)+1; //.Find<string>(types,nameofobject);
    }
    



}
