using Unity.Entities;

public static class MagicalTowerEcs
{
    public static EntityManager EntityManager => (World.DefaultGameObjectInjectionWorld ?? World.All[0]).EntityManager;
}
