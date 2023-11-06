public class YellowKey : BasePickup {
    void Start() {
        typeOfPickup = PickUpType.YellowKey;
        state = PickUpState.Idle;
    }
}