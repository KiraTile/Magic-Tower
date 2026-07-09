using UnityEngine;

[CreateAssetMenu(menuName = "Magical Tower/Game Config")]
public sealed class MagicalTowerConfigSO : ScriptableObject
{
    public TowerConfigSO tower;
    public SpawnScheduleSO spawnSchedule;
    public FireballSpellConfigSO fireball;
    public BarrageSpellConfigSO barrage;
    public EnemyConfigSO[] enemies;
}
