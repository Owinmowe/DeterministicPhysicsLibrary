using UnityEngine;

namespace DeterministicPhysicsLibrary.Runtime.Stateless
{
    public struct DSRigidbodyOutputData
    {
        public bool UsePredictedPosition;
        public Vector3 PredictedPosition;

        public bool UsePredictedRotation;
        public Quaternion PredictedRotation;

        public Vector3 Velocity;
        public Vector3 AngularVelocity;
        public Bounds Bounds;

        public bool Colliding;
        public Vector3 PenetrationDepthVector;
    }
}