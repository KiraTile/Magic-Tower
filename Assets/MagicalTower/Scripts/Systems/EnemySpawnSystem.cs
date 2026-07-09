using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(GameClockSystem))]
public partial struct EnemySpawnSystem : ISystem
{
    private const float MinimumSpawnInterval = 0.05f;
    private const float MinimumSpawnWeight = 0.01f;
    private const uint FallbackRandomState = 1u;
    private const int SpawnSideCount = 4;

    private EntityQuery _enemyQuery;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MagicalTowerGameState>();
        state.RequireForUpdate<SpawnState>();
        _enemyQuery = SystemAPI.QueryBuilder().WithAll<Enemy>().Build();
    }

    public void OnUpdate(ref SystemState state)
    {
        var game = SystemAPI.GetSingletonRW<MagicalTowerGameState>();
        if (game.ValueRO.IsGameOver || _enemyQuery.CalculateEntityCount() >= game.ValueRO.MaxEnemies)
        {
            return;
        }

        var spawnEntity = SystemAPI.GetSingletonEntity<SpawnState>();
        var spawn = SystemAPI.GetComponentRW<SpawnState>(spawnEntity);
        var definitions = SystemAPI.GetBuffer<EnemyDefinitionElement>(spawnEntity);
        var phases = SystemAPI.GetBuffer<SpawnPhaseElement>(spawnEntity);
        if (definitions.Length == 0 || phases.Length == 0)
        {
            return;
        }

        SpawnPhaseElement phase = phases[0];
        for (int i = 1; i < phases.Length; i++)
        {
            if (game.ValueRO.ElapsedTime >= phases[i].StartTime)
            {
                phase = phases[i];
            }
        }

        spawn.ValueRW.Timer -= SystemAPI.Time.DeltaTime;
        if (spawn.ValueRO.Timer > 0f)
        {
            return;
        }

        spawn.ValueRW.Timer = math.max(MinimumSpawnInterval, phase.SpawnInterval);
        var rng = new Random(game.ValueRO.RandomState == 0 ? FallbackRandomState : game.ValueRO.RandomState);
        float totalWeight = 0f;
        for (int i = 0; i < definitions.Length; i++)
        {
            totalWeight += math.max(MinimumSpawnWeight, definitions[i].SpawnWeight * phase.WeightMultiplier);
        }

        float roll = rng.NextFloat(0f, totalWeight);
        EnemyDefinitionElement definition = definitions[0];
        for (int i = 0; i < definitions.Length; i++)
        {
            roll -= math.max(MinimumSpawnWeight, definitions[i].SpawnWeight * phase.WeightMultiplier);
            if (roll <= 0f)
            {
                definition = definitions[i];
                break;
            }
        }

        float2 halfExtents = game.ValueRO.SpawnHalfExtents;
        float padding = game.ValueRO.SpawnPadding;
        float3 position = GetSpawnPosition(ref rng, halfExtents, padding, game.ValueRO.EnemySpawnHeight);
        game.ValueRW.RandomState = rng.state;

        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        Entity enemy = ecb.CreateEntity();
        ecb.AddComponent(enemy, LocalTransform.FromPositionRotationScale(position, quaternion.identity, definition.Scale));
        ecb.AddComponent(enemy, new Enemy
        {
            DefinitionId = definition.Id,
            Health = definition.Health * phase.HealthMultiplier,
            MaxHealth = definition.Health * phase.HealthMultiplier,
            MoveSpeed = definition.MoveSpeed * phase.SpeedMultiplier,
            ContactDamage = definition.ContactDamage,
            Radius = definition.Radius * definition.Scale
        });
        ecb.AddComponent(enemy, new EnemyView());
        Entity request = ecb.CreateEntity();
        ecb.AddComponent(request, new SpawnEnemyViewRequest
        {
            Target = enemy,
            DefinitionId = definition.Id,
            Position = position,
            Scale = definition.Scale
        });
    }

    private static float3 GetSpawnPosition(ref Random rng, float2 halfExtents, float padding, float height)
    {
        int side = rng.NextInt(0, SpawnSideCount);
        float x = rng.NextFloat(-halfExtents.x, halfExtents.x);
        float z = rng.NextFloat(-halfExtents.y, halfExtents.y);
        if (side == 0) x = -halfExtents.x - padding;
        if (side == 1) x = halfExtents.x + padding;
        if (side == 2) z = -halfExtents.y - padding;
        if (side == 3) z = halfExtents.y + padding;
        return new float3(x, height, z);
    }
}
