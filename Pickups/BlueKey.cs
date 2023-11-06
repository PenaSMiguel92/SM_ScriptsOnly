public class BlueKey : BasePickup {
    void Start() {
        typeOfPickup = PickUpType.BlueKey;
        state = PickUpState.Idle;
    }
}