using FixedPoint.SubTypes;
using System;
using UnityEngine;

namespace DeterministicPhysicsLibrary.Unity
{
    public class DMRigidbody : MonoBehaviour
    {
        public static event Func<DMRigidbody, int> OnRigidbodyEnabled;
        public static event Action<int> OnRigidbodyDisabled;

        [SerializeField] private DMRigidbodyData data;
        public DMRigidbodyData Data => data;

        private void OnEnable()
        {
            data.simulationData.rigidBodyIndex = OnRigidbodyEnabled.Invoke(this);
        }

        private void OnDisable()
        {
            OnRigidbodyDisabled?.Invoke(data.simulationData.rigidBodyIndex);
        }

        public void AddForce(Vector3Fp force) 
        {
            
        }
    }
}
