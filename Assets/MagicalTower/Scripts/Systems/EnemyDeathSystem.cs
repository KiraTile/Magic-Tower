using Unity.Entities;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(BurningSystem))]
public partial struct EnemyDeathSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MagicalTowerGameState>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        foreach (var (enemy, transform, view, entity) in SystemAPI.Query<RefRO<Enemy>, RefRO<LocalTransform>, RefRO<EnemyView>>().WithEntityAccess())
        {
            if (enemy.ValueRO.Health > 0f)
            {
                continue;
            }

            Entity damageRequest = ecb.CreateEntity();
            ecb.AddComponent(damageRequest, new ExplosionRequest { Position = transform.ValueRO.Position, Radius = enemy.ValueRO.Radius });
            if (view.ValueRO.Id != 0)
            {
                Entity release = ecb.CreateEntity();
                ecb.AddComponent(release, new ReleaseViewRequest { ViewId = view.ValueRO.Id, Kind = MagicalTowerViewKind.Enemy });
            }
            ecb.DestroyEntity(entity);
        }
    }
}
