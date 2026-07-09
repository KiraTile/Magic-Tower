using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public sealed class EnemyViewBridge : MonoBehaviour
{
    public MagicalTowerConfigSO config;
    public GameObject enemyViewPrefab;

    private readonly Dictionary<int, GameObject> _active = new();
    private readonly Queue<GameObject> _pool = new();
    private int _nextViewId = 1;

    private void LateUpdate()
    {
        EntityManager entityManager = MagicalTowerEcs.EntityManager;
        ProcessSpawnRequests(entityManager);
        ProcessReleaseRequests(entityManager);
        Sync(entityManager);
    }

    private void ProcessSpawnRequests(EntityManager entityManager)
    {
        EntityQuery query = entityManager.CreateEntityQuery(typeof(SpawnEnemyViewRequest));
        using NativeArray<Entity> entities = query.ToEntityArray(Allocator.Temp);
        using NativeArray<SpawnEnemyViewRequest> requests = query.ToComponentDataArray<SpawnEnemyViewRequest>(Allocator.Temp);
        for (int i = 0; i < requests.Length; i++)
        {
            int id = _nextViewId++;
            GameObject view = GetView(requests[i].DefinitionId, requests[i].Scale);
            view.transform.position = requests[i].Position;
            view.SetActive(true);
            _active[id] = view;
            entityManager.SetComponentData(requests[i].Target, new EnemyView { Id = id });
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
            if (requests[i].Kind != MagicalTowerViewKind.Enemy)
            {
                continue;
            }

            GameObject view = _active[requests[i].ViewId];
            _active.Remove(requests[i].ViewId);
            view.SetActive(false);
            _pool.Enqueue(view);
            entityManager.DestroyEntity(entities[i]);
        }

        query.Dispose();
    }

    private void Sync(EntityManager entityManager)
    {
        EntityQuery query = entityManager.CreateEntityQuery(typeof(Enemy), typeof(EnemyView), typeof(LocalTransform));
        using NativeArray<EnemyView> views = query.ToComponentDataArray<EnemyView>(Allocator.Temp);
        using NativeArray<LocalTransform> transforms = query.ToComponentDataArray<LocalTransform>(Allocator.Temp);
        for (int i = 0; i < views.Length; i++)
        {
            _active[views[i].Id].transform.position = transforms[i].Position;
        }

        query.Dispose();
    }

    private GameObject GetView(int definitionId, float scale)
    {
        GameObject view = _pool.Count > 0 ? _pool.Dequeue() : Instantiate(enemyViewPrefab);
        EnemyConfigSO enemy = config.enemies[definitionId];
        view.transform.localScale = new Vector3(scale, scale, scale);
        view.GetComponent<Renderer>().material.color = enemy.color;
        return view;
    }
}
