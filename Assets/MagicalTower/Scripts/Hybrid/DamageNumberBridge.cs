using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public sealed class DamageNumberBridge : MonoBehaviour
{
    public Camera gameplayCamera;
    public Canvas uiCanvas;
    public DamageNumberPopup damageNumberPrefab;

    private readonly Queue<DamageNumberPopup> _pool = new();

    private void LateUpdate()
    {
        EntityManager entityManager = MagicalTowerEcs.EntityManager;
        EntityQuery query = entityManager.CreateEntityQuery(typeof(DamageNumberRequest));
        using NativeArray<Entity> entities = query.ToEntityArray(Allocator.Temp);
        using NativeArray<DamageNumberRequest> requests = query.ToComponentDataArray<DamageNumberRequest>(Allocator.Temp);
        for (int i = 0; i < requests.Length; i++)
        {
            DamageNumberPopup popup = _pool.Count > 0 ? _pool.Dequeue() : Instantiate(damageNumberPrefab, uiCanvas.transform);
            popup.gameObject.SetActive(true);
            popup.Initialize(gameplayCamera, requests[i].Position, requests[i].Amount, _pool);
            entityManager.DestroyEntity(entities[i]);
        }

        query.Dispose();
    }
}
