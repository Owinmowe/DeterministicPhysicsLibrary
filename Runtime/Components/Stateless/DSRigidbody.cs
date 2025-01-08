using System;
using UnityEngine;
using FixedPoint.SubTypes;

namespace DeterministicPhysicsLibrary.Unity
{
    public class DSRigidbody : MonoBehaviour
    {
        public static event Func<DSRigidbody, int> OnRigidbodyEnabled;
        public static event Action<int> OnRigidbodyDisabled;

        [SerializeField] private DSRigidbodyData data;
        public DSRigidbodyData Data => data;

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
            data.simulationConfiguration.externalAcceleration += force;
        }
    }
}