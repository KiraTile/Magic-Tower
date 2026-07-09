using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(EnemyMoveSystem))]
[UpdateBefore(typeof(EnemyDeathSystem))]
public partial struct EnemyAttackTowerSystem : ISystem
{
    private const float DamageNumberHeightOffset = 2.4f;

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

        var tower = SystemAPI.GetSingletonRW<TowerState>();
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        foreach (var (enemy, transform) in SystemAPI.Query<RefRW<Enemy>, RefRO<LocalTransform>>())
        {
            if (enemy.ValueRO.Health <= 0f)
            {
                continue;
            }

            float distance = math.distance(transform.ValueRO.Position, tower.ValueRO.Position);
            if (distance > enemy.ValueRO.Radius + tower.ValueRO.Radius)
            {
                continue;
            }

            tower.ValueRW.Health = math.max(0f, tower.ValueRO.Health - enemy.ValueRO.ContactDamage);
            Entity damageRequest = ecb.CreateEntity();
            ecb.AddComponent(damageRequest, new DamageNumberRequest { Position = tower.ValueRO.Position + new float3(0f, DamageNumberHeightOffset, 0f), Amount = enemy.ValueRO.ContactDamage });
            enemy.ValueRW.Health = 0f;
        }
    }
}
