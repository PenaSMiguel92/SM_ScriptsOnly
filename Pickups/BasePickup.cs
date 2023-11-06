using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasePickup : MonoBehaviour, IPickup
{
    protected PickUpType typeOfPickup;
    public PickUpType TypeOfPickup { get { return typeOfPickup; } }
    protected PickUpState state = PickUpState.Loading;
    public PickUpState State {get { return state; } }
    protected Transform pickupTransform;

    protected GameControl mainControl;
    protected Player mainPlayer;
    // Start is called before the first frame update
    void Awake()
    {
        mainControl = GameControl.Main;
        mainPlayer = Player.Main;
        pickupTransform = gameObject.GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (state == PickUpState.Loading) return;
        if (state == PickUpState.PickedUp) Destroy(gameObject);
        if ((mainPlayer.GetPosition() - pickupTransform.localPosition).magnitude < 0.707)
        {
            mainPlayer.AddToInventory(typeOfPickup);
            state = PickUpState.PickedUp;
        }
    }
}
