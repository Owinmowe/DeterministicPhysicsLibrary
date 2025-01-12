using FixedPoint.SubTypes;
using DeterministicPhysicsLibrary.Runtime.Stateless;
using UnityEngine;

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
            data.Extents = (Vector3)configuration.collisionDetection.extents;
            data.Radius = (float)configuration.collisionDetection.radius;

            data.CollisionResponseType = configuration.collisionResponse.collisionResponseType;

            data.ExternalAcceleration = (Vector3)(configuration.dynamics.gravity + simulationConfiguration.externalAcceleration);
            data.ExternalAngularAcceleration = (Vector3)simulationConfiguration.externalAngularAcceleration;
            data.Mass = (float)configuration.dynamics.mass;

            simulationConfiguration.externalAcceleration = Vector3Fp.Zero;
            simulationConfiguration.externalAngularAcceleration = Vector3Fp.Zero;
            return data;
        }
    }
}