using FixedPoint;
using FixedPoint.SubTypes;

namespace DeterministicPhysicsLibrary.Runtime
{
    public struct DeterministicRigidbodyData
    {
        public int Index;

        public Vector3Fp Position;
        public QuaternionFp Rotation;

        public Vector3Fp Velocity;
        public Vector3Fp AngularVelocity;
        public Vector3Fp Acceleration;
        public Vector3Fp AngularAcceleration;

        public ColliderType ColliderType;
        public CollisionLayer CollisionLayer;
        public Fp Mass;
        public Fp Radius;
        public Vector3Fp Extents;
        public BoundsFp Bounds;

        public CollisionResponseType CollisionResponseType;

        public void SetSimulationData(DeterministicRigidbodyData data) 
        {
            this.Velocity = data.Velocity;
            this.AngularVelocity = data.AngularVelocity;
            this.Bounds = data.Bounds;
        }

        public void SetMutableData(DeterministicRigidbodyData data) 
        {
            this.Position = data.Position;
            this.Rotation = data.Rotation;

            this.Acceleration = data.Acceleration;
            this.AngularAcceleration = data.AngularAcceleration;
            this.Mass = data.Mass;

            this.ColliderType = data.ColliderType;
            this.CollisionLayer = data.CollisionLayer;
            this.Radius = data.Radius;
            this.Extents = data.Extents;

            this.CollisionResponseType = data.CollisionResponseType;
        } 
    }
}
