public class SmallExplosion : BaseExplosion
{
    void Update()
    {
        if (state == ExplosionState.Done) return;
        if ((mainPlayer.GetPosition() - curTransform.localPosition).magnitude <= 0.707)
        {
            mainPlayer.Kill(DeathType.Standard);
            state = ExplosionState.Done;
        }
        IEnemyAI[] _enemies = GetComponents<IEnemyAI>();
        foreach (IEnemyAI _enemy in _enemies)
        {
            if ((_enemy.LocalPosition - curTransform.localPosition).magnitude <= 0.707)
            {
                _enemy.Kill(DeathType.Standard);
                state = ExplosionState.Done;
                break;
            }
        }
    }
}
