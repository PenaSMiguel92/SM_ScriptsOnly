public class Chest : BasePickup {
    void Start() {
        typeOfPickup = PickUpType.CoinChest;
        state = PickUpState.Idle;
    }
}