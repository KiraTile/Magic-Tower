using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(EnemySpawnSystem))]
public partial struct EnemyMoveSystem : ISystem
{
    private const float MinimumMovementDistance = 0.001f;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MagicalTowerGameState>();
        state.RequireForUpdate<TowerState>();
    }

    public void OnUpdate(ref SystemState state)
    {
        if (SystemAPI.GetSingleton<MagicalTowerGameState>().IsGameOver)
        {
            return;
        }

        float dt = SystemAPI.Time.DeltaTime;
        float3 towerPosition = SystemAPI.GetSingleton<TowerState>().Position;
        foreach (var (enemy, transform) in SystemAPI.Query<RefRO<Enemy>, RefRW<LocalTransform>>())
        {
            float3 toTower = towerPosition - transform.ValueRO.Position;
            toTower.y = 0f;
            float distance = math.length(toTower);
            if (distance > MinimumMovementDistance)
            {
                transform.ValueRW.Position += toTower / distance * enemy.ValueRO.MoveSpeed * dt;
            }
        }
    }
}
