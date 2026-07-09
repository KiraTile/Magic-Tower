using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public sealed class MagicalTowerBootstrap : MonoBehaviour
{
    public MagicalTowerConfigSO config;
    public Camera gameplayCamera;
    public Transform towerVisual;

    [SerializeField] private uint initialRandomState = 12345u;

    private const uint DefaultRandomState = 1u;

    private void Start()
    {
        EntityManager entityManager = MagicalTowerEcs.EntityManager;
        ClearPreviousRun(entityManager);
        float2 halfExtents = GetCameraGroundHalfExtents(gameplayCamera, config.spawnSchedule.minimumPlayfieldHalfExtents);
        float3 towerPosition = towerVisual.position;

        Entity game = entityManager.CreateEntity();
        entityManager.AddComponentData(game, new MagicalTowerGameState
        {
            RandomState = initialRandomState == 0u ? DefaultRandomState : initialRandomState,
            SpawnHalfExtents = halfExtents,
            SpawnPadding = config.spawnSchedule.spawnPadding,
            EnemySpawnHeight = config.spawnSchedule.enemySpawnHeight,
            MaxEnemies = config.spawnSchedule.maxEnemies
        });
        entityManager.AddComponentData(game, new TowerState
        {
            Position = towerPosition,
            Health = config.tower.maxHealth,
            MaxHealth = config.tower.maxHealth,
            Radius = config.tower.radius
        });
        entityManager.AddComponentData(game, new SpawnState());
        entityManager.AddComponentData(game, new FireballSpellState
        {
            Cooldown = config.fireball.cooldown,
            Timer = config.fireball.initialDelay,
            LaunchHeight = config.fireball.launchHeight,
            ProjectileSpeed = config.fireball.projectileSpeed,
            ProjectileRadius = config.fireball.projectileRadius,
            Gravity = config.fireball.gravity,
            ProjectileLifetime = config.fireball.projectileLifetime,
            GroundImpactHeight = config.fireball.groundImpactHeight,
            ExplosionHeight = config.fireball.explosionHeight,
            Damage = config.fireball.damage,
            ExplosionRadius = config.fireball.explosionRadius
        });
        entityManager.AddComponentData(game, new BarrageSpellState
        {
            Cooldown = config.barrage.cooldown,
            Timer = config.barrage.initialDelay,
            LaunchHeight = config.barrage.launchHeight,
            Damage = config.barrage.damage,
            FlightTime = config.barrage.flightTime,
            ArcHeight = config.barrage.arcHeight,
            TargetHeightOffset = config.barrage.targetHeightOffset,
            ProjectileRadius = config.barrage.projectileRadius,
            HitRadius = config.barrage.hitRadius
        });

        DynamicBuffer<SpellHitEffectElement> hitEffects = entityManager.AddBuffer<SpellHitEffectElement>(game);
        AddHitEffects(hitEffects, AttackKind.Fireball, config.fireball);
        AddHitEffects(hitEffects, AttackKind.Barrage, config.barrage);

        DynamicBuffer<EnemyDefinitionElement> enemies = entityManager.AddBuffer<EnemyDefinitionElement>(game);
        for (int i = 0; i < config.enemies.Length; i++)
        {
            EnemyConfigSO enemy = config.enemies[i];
            enemies.Add(new EnemyDefinitionElement
            {
                Id = i,
                Health = enemy.maxHealth,
                MoveSpeed = enemy.moveSpeed,
                ContactDamage = enemy.contactDamage,
                Radius = enemy.radius,
                Scale = enemy.scale,
                SpawnWeight = enemy.spawnWeight
            });
        }

        DynamicBuffer<SpawnPhaseElement> phases = entityManager.AddBuffer<SpawnPhaseElement>(game);
        foreach (SpawnPhase phase in config.spawnSchedule.phases)
        {
            phases.Add(new SpawnPhaseElement
            {
                StartTime = phase.startTime,
                SpawnInterval = phase.spawnInterval,
                HealthMultiplier = phase.healthMultiplier,
                SpeedMultiplier = phase.speedMultiplier,
                WeightMultiplier = phase.weightMultiplier
            });
        }
    }

    private static void AddHitEffects(DynamicBuffer<SpellHitEffectElement> buffer, AttackKind attackKind, ProjectileSpellConfigSO spell)
    {
        foreach (HitEffectSO effect in spell.hitEffects)
        {
            buffer.Add(new SpellHitEffectElement { AttackKind = attackKind, Effect = effect.ToRuntimeEffect() });
        }
    }

    private static void ClearPreviousRun(EntityManager entityManager)
    {
        DestroyQuery<MagicalTowerGameState>(entityManager);
        DestroyQuery<Enemy>(entityManager);
        DestroyQuery<FireballProjectile>(entityManager);
        DestroyQuery<BarrageProjectile>(entityManager);
        DestroyQuery<SpawnEnemyViewRequest>(entityManager);
        DestroyQuery<SpawnProjectileViewRequest>(entityManager);
        DestroyQuery<ReleaseViewRequest>(entityManager);
        DestroyQuery<DamageNumberRequest>(entityManager);
        DestroyQuery<ExplosionRequest>(entityManager);
    }

    private static void DestroyQuery<T>(EntityManager entityManager) where T : unmanaged, IComponentData
    {
        EntityQuery query = entityManager.CreateEntityQuery(ComponentType.ReadOnly<T>());
        entityManager.DestroyEntity(query);
        query.Dispose();
    }

    private static float2 GetCameraGroundHalfExtents(Camera camera, Vector2 minimumHalfExtents)
    {
        var plane = new Plane(Vector3.up, Vector3.zero);
        Vector3 min = Vector3.positiveInfinity;
        Vector3 max = Vector3.negativeInfinity;
        AddViewportCorner(camera, plane, new Vector3(0f, 0f, 0f), ref min, ref max);
        AddViewportCorner(camera, plane, new Vector3(1f, 0f, 0f), ref min, ref max);
        AddViewportCorner(camera, plane, new Vector3(0f, 1f, 0f), ref min, ref max);
        AddViewportCorner(camera, plane, new Vector3(1f, 1f, 0f), ref min, ref max);
        return math.max(
            new float2(minimumHalfExtents.x, minimumHalfExtents.y),
            new float2(math.abs(max.x - min.x) * 0.5f, math.abs(max.z - min.z) * 0.5f));
    }

    private static void AddViewportCorner(Camera camera, Plane plane, Vector3 viewport, ref Vector3 min, ref Vector3 max)
    {
        Ray ray = camera.ViewportPointToRay(viewport);
        plane.Raycast(ray, out float enter);
        Vector3 point = ray.GetPoint(enter);
        min = Vector3.Min(min, point);
        max = Vector3.Max(max, point);
    }
}
