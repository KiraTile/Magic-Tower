using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct BurningSystem : ISystem
{
    private const float MinimumTickInterval = 0.01f;
    private const float DamageNumberHeightOffset = 1.4f;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MagicalTowerGameState>();
    }

    public void OnUpdate(ref SystemState state)
    {
        if (SystemAPI.GetSingleton<MagicalTowerGameState>().IsGameOver)
        {
            return;
        }

        float dt = SystemAPI.Time.DeltaTime;
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        foreach (var (enemy, burning, transform, entity) in SystemAPI.Query<RefRW<Enemy>, RefRW<Burning>, RefRO<LocalTransform>>().WithEntityAccess())
        {
            float remaining = math.max(0f, burning.ValueRO.Remaining);
            float tickInterval = math.max(MinimumTickInterval, burning.ValueRO.TickInterval);
            float tickTimer = burning.ValueRO.TickTimer > 0f ? math.min(burning.ValueRO.TickTimer, tickInterval) : tickInterval;
            float elapsed = math.min(dt, remaining);
            float activeTime = elapsed;
            float damageTime = 0f;
            while (activeTime >= tickTimer)
            {
                damageTime += tickInterval;
                activeTime -= tickTimer;
                tickTimer = tickInterval;
            }

            tickTimer -= activeTime;
            remaining -= elapsed;
            if (remaining <= 0f)
            {
                damageTime += tickInterval - tickTimer;
            }

            float damage = burning.ValueRO.DamagePerSecond * damageTime;
            if (damage > 0f)
            {
                enemy.ValueRW.Health = math.max(0f, enemy.ValueRO.Health - damage);
                Entity damageRequest = ecb.CreateEntity();
                ecb.AddComponent(damageRequest, new DamageNumberRequest { Position = transform.ValueRO.Position + new float3(0f, DamageNumberHeightOffset, 0f), Amount = damage });
            }

            burning.ValueRW.Remaining = remaining;
            burning.ValueRW.TickInterval = tickInterval;
            burning.ValueRW.TickTimer = tickTimer;
            if (remaining <= 0f)
            {
                ecb.RemoveComponent<Burning>(entity);
            }
        }
    }
}
