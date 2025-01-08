using FixedPoint.SubTypes;

namespace DeterministicPhysicsLibrary.Runtime.Managed
{
    public struct DMRigidbodySimulationData
    {
        public Vector3Fp Position;
        public QuaternionFp Rotation;

        public Vector3Fp Velocity;
        public Vector3Fp AngularVelocity;

        public Vector3Fp Acceleration;
        public Vector3Fp AngularAcceleration;

        public BoundsFp Bounds;
    }
}