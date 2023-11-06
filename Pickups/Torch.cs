public class Torch : BasePickup {
    void Start() {
        typeOfPickup = PickUpType.Torch;
        state = PickUpState.Idle;
    }
}