using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public sealed class ExplosionViewBridge : MonoBehaviour
{
    public GameObject explosionViewPrefab;

    [SerializeField, Min(0f)] private float verticalOffset = 0.35f;
    [SerializeField, Min(0.01f)] private float lifetime = 0.28f;
    [SerializeField, Min(0f)] private float minimumViewRadius = 0.6f;
    [SerializeField, Range(0f, 1f)] private float shrinkMultiplierPerFrame = 0.92f;

    private readonly Queue<GameObject> _pool = new();
    private readonly List<TimedView> _active = new();

    private const float RadiusToDiameter = 2f;
    private const float ReferenceFramesPerSecond = 60f;

    private void LateUpdate()
    {
        EntityManager entityManager = MagicalTowerEcs.EntityManager;
        ProcessRequests(entityManager);
        TickActive();
    }

    private void ProcessRequests(EntityManager entityManager)
    {
        EntityQuery query = entityManager.CreateEntityQuery(typeof(ExplosionRequest));
        using NativeArray<Entity> entities = query.ToEntityArray(Allocator.Temp);
        using NativeArray<ExplosionRequest> requests = query.ToComponentDataArray<ExplosionRequest>(Allocator.Temp);
        for (int i = 0; i < requests.Length; i++)
        {
            GameObject view = _pool.Count > 0 ? _pool.Dequeue() : Instantiate(explosionViewPrefab);
            view.transform.position = requests[i].Position + new Unity.Mathematics.float3(0f, verticalOffset, 0f);
            view.transform.localScale = Vector3.one * Mathf.Max(minimumViewRadius, requests[i].Radius) * RadiusToDiameter;
            view.SetActive(true);
            _active.Add(new TimedView { View = view, Remaining = lifetime });
            entityManager.DestroyEntity(entities[i]);
        }

        query.Dispose();
    }

    private void TickActive()
    {
        for (int i = _active.Count - 1; i >= 0; i--)
        {
            TimedView item = _active[i];
            item.Remaining -= Time.deltaTime;
            if (item.Remaining <= 0f)
            {
                item.View.SetActive(false);
                _pool.Enqueue(item.View);
                _active.RemoveAt(i);
                continue;
            }

            item.View.transform.localScale *= Mathf.Pow(shrinkMultiplierPerFrame, Time.deltaTime * ReferenceFramesPerSecond);
            _active[i] = item;
        }
    }

    private struct TimedView
    {
        public GameObject View;
        public float Remaining;
    }
}
