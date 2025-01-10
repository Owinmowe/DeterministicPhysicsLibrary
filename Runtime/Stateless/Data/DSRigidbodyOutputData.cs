using UnityEngine;

namespace DeterministicPhysicsLibrary.Runtime.Stateless
{
    public struct DSRigidbodyOutputData
    {
        public Vector3 PredictedPosition;
        public Quaternion PredictedRotation;
        public Vector3 Velocity;
        public Vector3 AngularVelocity;
        public Bounds Bounds;
        public bool Colliding;

        public Vector3 ClosestPointWorld;
    }
}