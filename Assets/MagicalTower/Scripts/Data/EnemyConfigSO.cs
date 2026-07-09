using UnityEngine;

[CreateAssetMenu(menuName = "Magical Tower/Enemy Config")]
public sealed class EnemyConfigSO : ScriptableObject
{
    public string enemyName = "Enemy";
    public float maxHealth = 20f;
    public float moveSpeed = 2.5f;
    public float contactDamage = 5f;
    public float radius = 0.45f;
    public float scale = 1f;
    public float spawnWeight = 1f;
    public Color color = Color.green;
}
