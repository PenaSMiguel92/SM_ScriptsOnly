//Types
public enum DeathType { Standard, Burn, Electricution, Acid }
public enum SoundType {BigExplosion, Boomerang, Chest, Pickup, Death, DoorOpen, Explosion, Flame, Lightbeam, 
                       SetPushable, NinjaSpin, PassLevel, PistolShot, Push, ShurikenThrow, ShurikenHit}
public enum EnemyType { Guard, Gatherer, Ninja, Mutant, Clone }
public enum PickUpType { Coin, CoinChest, RedKey, BlueKey, YellowKey, GreenKey, Bomb, Torch, None }
public enum TrapType { Hole, ElectricalBox, BudaStatue}

//States
public enum PlayerState { Loading, Idle, Walking, Sliding, Death };
public enum PushState {Loading, Idle, Moving, Crossing, Set}
public enum PickUpState {Loading, Idle, PickedUp}
public enum BombState { Idle, Waiting, Moving, Exploding, Exploded}
public enum CameraState { Loading, Following}
public enum ConvState {Loading, Idle}
public enum EnemyState { Loading, Idle, Walk, Attack, Follow, Sliding, Death }
public enum ExplosionState {Searching, Done}
public enum GameState {Loading, Cutscene, Menu, Pause, LevelPlay, LevelEnd}
public enum TrapState {Loading, Idle, Waiting, Crossing, Set, PlayerStand} 
//Sections
public enum FlameSection { Mid, End };

//Directions
public enum ConvDir {Up, Down, Left, Right}
public enum ShootDirection { Up, Right, Down, Left };
public enum InventoryDirection {Left, Right}

//Usage
public enum TilemapUse { Foreground, Moveables, Enemies }