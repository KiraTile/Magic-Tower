using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Magical Tower/Spawn Schedule")]
public sealed class SpawnScheduleSO : ScriptableObject
{
    public float spawnPadding = 4f;
    [Min(0f)] public float enemySpawnHeight = 0.45f;
    public Vector2 minimumPlayfieldHalfExtents = new(6f, 4f);
    public int maxEnemies = 90;
    public SpawnPhase[] phases =
    {
        new SpawnPhase { startTime = 0f, spawnInterval = 1.5f, healthMultiplier = 1f, speedMultiplier = 1f, weightMultiplier = 1f },
        new SpawnPhase { startTime = 45f, spawnInterval = 1.0f, healthMultiplier = 1.25f, speedMultiplier = 1.08f, weightMultiplier = 1.15f },
        new SpawnPhase { startTime = 90f, spawnInterval = 0.65f, healthMultiplier = 1.55f, speedMultiplier = 1.16f, weightMultiplier = 1.35f }
    };
}

[Serializable]
public struct SpawnPhase
{
    public float startTime;
    public float spawnInterval;
    public float healthMultiplier;
    public float speedMultiplier;
    public float weightMultiplier;
}
