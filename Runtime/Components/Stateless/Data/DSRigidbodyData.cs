using FixedPoint.SubTypes;
using DeterministicPhysicsLibrary.Runtime.Stateless;

namespace DeterministicPhysicsLibrary.Unity
{
    [System.Serializable]
    public class DSRigidbodyData
    {
        public DRigidbodyConfiguration configuration;
        public DRigidbodySimulationData simulationConfiguration;

        public DSRigidbodyInputData GetInputData() 
        {
            DSRigidbodyInputData data = new();

            data.ColliderType = configuration.collisionDetection.colliderType;
            data.CollisionLayer = configuration.collisionDetection.collisionLayer;
            data.Extents = configuration.collisionDetection.extents;
            data.Radius = configuration.collisionDetection.radius;

            data.ExternalAcceleration = configuration.dynamics.gravity + simulationConfiguration.externalAcceleration;
            data.ExternalAngularAcceleration = simulationConfiguration.externalAngularAcceleration;
            data.Mass = configuration.dynamics.mass;

            simulationConfiguration.externalAcceleration = Vector3Fp.Zero;
            simulationConfiguration.externalAngularAcceleration = Vector3Fp.Zero;
            return data;
        }
    }
}