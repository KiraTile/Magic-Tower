using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class MagicalTowerConfigurationDefaultsTests
{
    [Test]
    public void AuthoredAssetsPreserveMigratedTuning()
    {
        FireballSpellConfigSO fireball = AssetDatabase.LoadAssetAtPath<FireballSpellConfigSO>("Assets/MagicalTower/Data/Fireball.asset");
        BarrageSpellConfigSO barrage = AssetDatabase.LoadAssetAtPath<BarrageSpellConfigSO>("Assets/MagicalTower/Data/Barrage.asset");
        SpawnScheduleSO spawnSchedule = AssetDatabase.LoadAssetAtPath<SpawnScheduleSO>("Assets/MagicalTower/Data/SpawnSchedule.asset");

        Assert.That(fireball.initialDelay, Is.EqualTo(0.4f));
        Assert.That(fireball.launchHeight, Is.EqualTo(1.6f));
        Assert.That(fireball.projectileLifetime, Is.EqualTo(4f));
        Assert.That(fireball.groundImpactHeight, Is.EqualTo(0.2f));
        Assert.That(fireball.explosionHeight, Is.EqualTo(0.45f));
        Assert.That(barrage.initialDelay, Is.EqualTo(2f));
        Assert.That(barrage.launchHeight, Is.EqualTo(1.4f));
        Assert.That(barrage.targetHeightOffset, Is.EqualTo(0.45f));
        Assert.That(spawnSchedule.enemySpawnHeight, Is.EqualTo(0.45f));
        Assert.That(spawnSchedule.minimumPlayfieldHalfExtents, Is.EqualTo(new Vector2(6f, 4f)));
    }
}
