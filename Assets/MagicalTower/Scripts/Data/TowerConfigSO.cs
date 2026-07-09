using UnityEngine;

[CreateAssetMenu(menuName = "Magical Tower/Tower Config")]
public sealed class TowerConfigSO : ScriptableObject
{
    public float maxHealth = 250f;
    public float radius = 1.15f;
}
