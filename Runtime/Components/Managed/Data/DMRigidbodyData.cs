using FixedPoint.SubTypes;
using DeterministicPhysicsLibrary.Runtime.Managed;

namespace DeterministicPhysicsLibrary.Unity
{
    [System.Serializable]
    public class DMRigidbodyData
    {
        public DRigidbodyConfiguration configuration;
        public DRigidbodySimulationData simulationData;

        public DMRigidbodyInputData GetInputData()
        {
            DMRigidbodyInputData data = new();

            data.ColliderType = configuration.collisionDetection.colliderType;
            data.CollisionLayer = configuration.collisionDetection.collisionLayer;
            data.Extents = configuration.collisionDetection.extents;
            data.Radius = configuration.collisionDetection.radius;

            data.Mass = configuration.dynamics.mass;

            data.ExternalAcceleration = configuration.dynamics.gravity + simulationData.externalAcceleration;
            data.ExternalAngularAcceleration = simulationData.externalAngularAcceleration;

            simulationData.externalAcceleration = Vector3Fp.Zero;
            simulationData.externalAngularAcceleration = Vector3Fp.Zero;

            return data;
        }
    }
}