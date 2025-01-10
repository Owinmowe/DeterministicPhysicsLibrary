using UnityEngine;

namespace DeterministicPhysicsLibrary.Runtime.Stateless
{
    public struct DSRigidbodyInputData
    {
        public Vector3 Position;
        public Quaternion Rotation;

        public Vector3 ExternalAcceleration;
        public Vector3 ExternalAngularAcceleration;

        public ColliderType ColliderType;
        public CollisionLayer CollisionLayer;
        public float Mass;
        public float Radius;
        public Vector3 Extents;

        public CollisionResponseType CollisionResponseType;
    }
}