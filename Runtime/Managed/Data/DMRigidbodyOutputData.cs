using FixedPoint.SubTypes;

namespace DeterministicPhysicsLibrary.Runtime.Managed
{
    public struct DMRigidbodyOutputData
    {
        public Vector3Fp Position;
        public QuaternionFp Rotation;
        public BoundsFp Bounds;
        public bool Colliding;
    }
}