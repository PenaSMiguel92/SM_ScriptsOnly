public class RedKey : BasePickup {
    void Start() {
        typeOfPickup = PickUpType.RedKey;
        state = PickUpState.Idle;
    }
}