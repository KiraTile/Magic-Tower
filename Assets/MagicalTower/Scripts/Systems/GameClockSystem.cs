using Unity.Entities;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct GameClockSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MagicalTowerGameState>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var game = SystemAPI.GetSingletonRW<MagicalTowerGameState>();
        if (game.ValueRO.IsGameOver)
        {
            return;
        }

        game.ValueRW.ElapsedTime += SystemAPI.Time.DeltaTime;
    }
}
