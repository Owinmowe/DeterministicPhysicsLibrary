using FixedPoint;
using FixedPoint.SubTypes;

namespace DeterministicPhysicsLibrary.Runtime.Managed 
{
    public struct DMRigidbodyInputData
    {
        public ColliderType ColliderType;
        public CollisionLayer CollisionLayer;
        public Fp Mass;
        public Fp Radius;
        public Vector3Fp Extents;

        public Vector3Fp ExternalAcceleration;
        public Vector3Fp ExternalAngularAcceleration;

        public CollisionResponseType CollisionResponseType;
    }
}
