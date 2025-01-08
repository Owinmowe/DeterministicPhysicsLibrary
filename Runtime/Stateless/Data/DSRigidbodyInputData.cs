using FixedPoint;
using FixedPoint.SubTypes;

namespace DeterministicPhysicsLibrary.Runtime.Stateless
{
    public struct DSRigidbodyInputData
    {
        public Vector3Fp Position;
        public QuaternionFp Rotation;

        public Vector3Fp ExternalAcceleration;
        public Vector3Fp ExternalAngularAcceleration;

        public ColliderType ColliderType;
        public CollisionLayer CollisionLayer;
        public Fp Mass;
        public Fp Radius;
        public Vector3Fp Extents;

        public CollisionResponseType CollisionResponseType;
    }
}