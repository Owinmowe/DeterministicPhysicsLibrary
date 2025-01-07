namespace DeterministicPhysicsLibrary.Runtime
{
    [System.Flags]
    public enum CollisionResponseType
    {
        TriggerEvents = 1 << 0,
        Kinematic = 1 << 1
    }
}