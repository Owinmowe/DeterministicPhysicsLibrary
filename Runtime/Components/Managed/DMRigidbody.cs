using FixedPoint.SubTypes;
using UnityEngine;

namespace DeterministicPhysicsLibrary.Unity
{
    public class DMRigidbody : MonoBehaviour
    {
        [SerializeField] private DMRigidbodyData data;
        public DMRigidbodyData Data => data;

        private void OnEnable()
        {
            data.simulationData.rigidBodyIndex = DMPhysicsSystem.AddRigidbody(this);
        }

        private void OnDisable()
        {
            DMPhysicsSystem.RemoveRigidbody(data.simulationData.rigidBodyIndex);
        }

        public void AddForce(Vector3Fp force) 
        {
            
        }
    }
}
