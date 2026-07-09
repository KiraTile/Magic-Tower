using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public sealed class ProjectileViewBridge : MonoBehaviour
{
    public GameObject fireballViewPrefab;
    public GameObject barrageViewPrefab;

    [SerializeField, Min(0f)] private float minimumViewScale = 0.1f;

    private readonly Dictionary<int, GameObject> _active = new();
    private readonly Queue<GameObject> _fireballPool = new();
    private readonly Queue<GameObject> _barragePool = new();
    private int _nextViewId = 1;

    private const float RadiusToDiameter = 2f;

    private void LateUpdate()
    {
        EntityManager entityManager = MagicalTowerEcs.EntityManager;
        ProcessSpawnRequests(entityManager);
        ProcessReleaseRequests(entityManager);
        Sync(entityManager);
    }

    private void ProcessSpawnRequests(EntityManager entityManager)
    {
        EntityQuery query = entityManager.CreateEntityQuery(typeof(SpawnProjectileViewRequest));
        using NativeArray<Entity> entities = query.ToEntityArray(Allocator.Temp);
        using NativeArray<SpawnProjectileViewRequest> requests = query.ToComponentDataArray<SpawnProjectileViewRequest>(Allocator.Temp);
        for (int i = 0; i < requests.Length; i++)
        {
            int id = _nextViewId++;
            GameObject view = GetView(requests[i].Kind, requests[i].Scale);
            view.transform.position = requests[i].Position;
            view.SetActive(true);
            _active[id] = view;
            entityManager.SetComponentData(requests[i].Target, new ProjectileView { Id = id, Kind = requests[i].Kind });
            entityManager.DestroyEntity(entities[i]);
        }

        query.Dispose();
    }

    private void ProcessReleaseRequests(EntityManager entityManager)
    {
        EntityQuery query = entityManager.CreateEntityQuery(typeof(ReleaseViewRequest));
        using NativeArray<Entity> entities = query.ToEntityArray(Allocator.Temp);
        using NativeArray<ReleaseViewRequest> requests = query.ToComponentDataArray<ReleaseViewRequest>(Allocator.Temp);
        for (int i = 0; i < requests.Length; i++)
        {
            if (requests[i].Kind == MagicalTowerViewKind.Enemy)
            {
                continue;
            }

            GameObject view = _active[requests[i].ViewId];
            _active.Remove(requests[i].ViewId);
            view.SetActive(false);
            GetPool(requests[i].Kind).Enqueue(view);
            entityManager.DestroyEntity(entities[i]);
        }

        query.Dispose();
    }

    private void Sync(EntityManager entityManager)
    {
        EntityQuery query = entityManager.CreateEntityQuery(typeof(ProjectileView), typeof(LocalTransform));
        using NativeArray<ProjectileView> views = query.ToComponentDataArray<ProjectileView>(Allocator.Temp);
        using NativeArray<LocalTransform> transforms = query.ToComponentDataArray<LocalTransform>(Allocator.Temp);
        for (int i = 0; i < views.Length; i++)
        {
            _active[views[i].Id].transform.position = transforms[i].Position;
        }

        query.Dispose();
    }

    private GameObject GetView(int kind, float scale)
    {
        Queue<GameObject> pool = GetPool(kind);
        GameObject view = pool.Count > 0 ? pool.Dequeue() : Instantiate(GetPrefab(kind));
        view.transform.localScale = Vector3.one * math.max(minimumViewScale, scale * RadiusToDiameter);
        return view;
    }

    private Queue<GameObject> GetPool(int kind)
    {
        return kind == MagicalTowerViewKind.Fireball ? _fireballPool : _barragePool;
    }

    private GameObject GetPrefab(int kind)
    {
        return kind == MagicalTowerViewKind.Fireball ? fireballViewPrefab : barrageViewPrefab;
    }
}
