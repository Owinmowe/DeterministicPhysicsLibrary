using UnityEngine;
using FixedPoint.SubTypes;

namespace DeterministicPhysicsLibrary.Unity
{
    public class DSRigidbody : MonoBehaviour
    {

        [SerializeField] private DSRigidbodyData data;
        public DSRigidbodyData Data => data;

        private void OnEnable()
        {
            data.simulationConfiguration.rigidBodyIndex = DSPhysicsSystem.AddRigidbody(this);
        }

        private void OnDisable()
        {
            DSPhysicsSystem.RemoveRigidbody(data.simulationConfiguration.rigidBodyIndex);
        }

        public void AddForce(Vector3Fp force)
        {
            data.simulationConfiguration.externalAcceleration += force;
        }
    }
}