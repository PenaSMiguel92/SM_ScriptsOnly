using UnityEngine;

public interface IPushable
{
    public PushState State { get; set; }
    public TrapType Type { get; }
    public Vector3Int MoveDirection { get; }
    public Vector3Int TileLocation { get; }
    public bool TestPush(Vector3Int _plrMoveDirection);
    public bool ForcePush(Vector3Int _moveDirection, float _speed);
    public Vector3 GetPosition();
}
