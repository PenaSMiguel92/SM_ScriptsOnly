public class Coin : BasePickup {
    void Start() {
        typeOfPickup = PickUpType.Coin;
        state = PickUpState.Idle;
    }
}