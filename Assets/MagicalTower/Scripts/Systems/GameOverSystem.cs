using Unity.Entities;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(EnemyAttackTowerSystem))]
public partial struct GameOverSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MagicalTowerGameState>();
        state.RequireForUpdate<TowerState>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var game = SystemAPI.GetSingletonRW<MagicalTowerGameState>();
        if (game.ValueRO.IsGameOver || SystemAPI.GetSingleton<TowerState>().Health > 0f)
        {
            return;
        }

        game.ValueRW.IsGameOver = true;
    }
}
