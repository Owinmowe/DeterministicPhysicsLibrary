using FixedPoint.SubTypes;
using System;
using UnityEngine;

namespace DeterministicPhysicsLibrary.Unity
{
    public class DMRigidbody : MonoBehaviour
    {
        public static event Func<DMRigidbody, ulong> OnRigidbodyEnabled;
        public static event Action<ulong> OnRigidbodyDisabled;

        [SerializeField] private DMRigidbodyData data;
        public DMRigidbodyData Data => data;

        private void OnEnable()
        {
            data.simulationConfiguration.rigidBodyIndex = OnRigidbodyEnabled.Invoke(this);
        }

        private void OnDisable()
        {
            OnRigidbodyDisabled?.Invoke(data.simulationConfiguration.rigidBodyIndex);
        }

        public void AddForce(Vector3Fp force) 
        {
            data.simulationConfiguration.acceleration += force;
        }
    }
}
