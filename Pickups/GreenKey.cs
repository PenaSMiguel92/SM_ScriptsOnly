public class GreenKey : BasePickup {
    void Start() {
        typeOfPickup = PickUpType.GreenKey;
        state = PickUpState.Idle;
    }
}