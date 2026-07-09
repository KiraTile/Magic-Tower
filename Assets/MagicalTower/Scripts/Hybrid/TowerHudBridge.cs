using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public sealed class TowerHudBridge : MonoBehaviour
{
    public RectTransform healthFill;
    public Text gameOverText;

    private void Start()
    {
        SetHealth(1f);
        gameOverText.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        EntityManager entityManager = MagicalTowerEcs.EntityManager;
        EntityQuery query = entityManager.CreateEntityQuery(typeof(MagicalTowerGameState), typeof(TowerState));
        Entity entity = query.GetSingletonEntity();
        TowerState tower = entityManager.GetComponentData<TowerState>(entity);
        MagicalTowerGameState game = entityManager.GetComponentData<MagicalTowerGameState>(entity);
        query.Dispose();
        SetHealth(tower.Health / tower.MaxHealth);
        gameOverText.gameObject.SetActive(game.IsGameOver);
    }

    private void SetHealth(float value)
    {
        Vector2 anchorMax = healthFill.anchorMax;
        anchorMax.x = math.saturate(value);
        healthFill.anchorMax = anchorMax;
    }
}
