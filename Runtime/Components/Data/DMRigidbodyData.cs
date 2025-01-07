using DeterministicPhysicsLibrary.Runtime;
using FixedPoint.SubTypes;

namespace DeterministicPhysicsLibrary.Unity
{
    [System.Serializable]
    public class DMRigidbodyData
    {
        public DMRigidbodyConfiguration configuration;
        public DMRigidbodySimulationData simulationConfiguration;

        public DeterministicRigidbodyData GetMutableData()
        {
            DeterministicRigidbodyData data = new();

            data.ColliderType = configuration.collisionDetection.colliderType;
            data.CollisionLayer = configuration.collisionDetection.collisionLayer;
            data.Extents = configuration.collisionDetection.extents;
            data.Radius = configuration.collisionDetection.radius;

            data.Acceleration = configuration.dynamics.gravity + simulationConfiguration.acceleration;
            data.Mass = configuration.dynamics.mass;

            simulationConfiguration.acceleration = Vector3Fp.Zero;
            simulationConfiguration.angularAcceleration = Vector3Fp.Zero;
            return data;
        }
    }
}