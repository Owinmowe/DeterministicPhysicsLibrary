namespace DeterministicPhysicsLibrary.Runtime
{
    [System.Flags]
    public enum CollisionResponseType
    {
        TriggerEvents = 1 << 0,
        Kinematic = 1 << 1
    }

    public static class CollisionResponseUtils 
    {
        public static bool HasFlag(CollisionResponseType type, CollisionResponseType flag)
        {
            return EnumTools.HasFlagUnsafe(type, flag);
        }
    }
}