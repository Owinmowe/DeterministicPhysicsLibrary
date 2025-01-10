using FixedPoint.SubTypes;

namespace DeterministicPhysicsLibrary.Runtime.Stateless
{
    public struct DSRigidbodyOutputData
    {
        public Vector3Fp PredictedPosition;
        public QuaternionFp PredictedRotation;
        public Vector3Fp Velocity;
        public Vector3Fp AngularVelocity;
        public BoundsFp Bounds;
        public bool Colliding;

        public Vector3Fp ClosestPointWorld;
    }
}