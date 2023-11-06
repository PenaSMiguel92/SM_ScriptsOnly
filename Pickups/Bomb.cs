public class Bomb : BasePickup {
    void Start() {
        typeOfPickup = PickUpType.Bomb;
        state = PickUpState.Idle;
    }
}