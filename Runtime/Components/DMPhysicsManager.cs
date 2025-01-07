using System.Collections.Generic;
using DeterministicPhysicsLibrary.Runtime;
using FixedPoint;
using FixedPoint.SubTypes;
using UnityEngine;

namespace DeterministicPhysicsLibrary.Unity
{
    public class DMPhysicsManager : MonoBehaviour
    {
        [SerializeField] private DeterministicSimulationConfiguration configuration;

        private static ManagedDeterministicSimulation _managedSimulation;
        private static readonly Dictionary<ulong, DMRigidbody> _rigidbodiesComponents = new();

        Dictionary<ulong, DeterministicRigidbodyData> _deterministicRigidbodies = new();

        [RuntimeInitializeOnLoadMethod(loadType: RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnLoadScene()
        {
            DMRigidbody.OnRigidbodyEnabled += AddRigidbody;
            DMRigidbody.OnRigidbodyDisabled += RemoveRigidbody;

            _managedSimulation = new ManagedDeterministicSimulation();
        }

        private void Awake()
        {
            _managedSimulation.RigidbodyUpdateCompleteEvent += ReceiveRigidbodyDataFromSimulation;
        }

        private void FixedUpdate()
        {
            UpdateSimulation();
        }

        private void OnDestroy()
        {
            _managedSimulation.RigidbodyUpdateCompleteEvent -= ReceiveRigidbodyDataFromSimulation;
            _managedSimulation = null;

            DMRigidbody.OnRigidbodyEnabled -= AddRigidbody;
            DMRigidbody.OnRigidbodyDisabled -= RemoveRigidbody;
        }

        private static ulong AddRigidbody(DMRigidbody rigidBodyComponent)
        {
            DeterministicRigidbodyData data = rigidBodyComponent.Data.GetMutableData();

            data.Position = (Vector3Fp)rigidBodyComponent.transform.position;
            data.Rotation = (QuaternionFp)rigidBodyComponent.transform.rotation;

            ulong rigidbodyIndex = _managedSimulation.AddRigidbody(data);
            
            _rigidbodiesComponents.Add(rigidbodyIndex, rigidBodyComponent);
            return rigidbodyIndex;
        }

        private static void RemoveRigidbody(ulong rigidbodyIndex)
        {
            _managedSimulation.RemoveRigidbody(rigidbodyIndex);
            _rigidbodiesComponents.Remove(rigidbodyIndex);
        }

        private void ReceiveRigidbodyDataFromSimulation(Dictionary<ulong, DeterministicRigidbodyData> deterministicRigidbodies)
        {
            foreach (var rigidBody in deterministicRigidbodies)
            {
                Vector3 positionUpdate = (Vector3)(rigidBody.Value.Velocity) * Time.fixedDeltaTime;
                _rigidbodiesComponents[rigidBody.Key].transform.position += positionUpdate;

                Vector3 rotationUpdate = (Vector3)(rigidBody.Value.AngularVelocity) * Time.fixedDeltaTime;
                _rigidbodiesComponents[rigidBody.Key].transform.rotation *= Quaternion.Euler(rotationUpdate);
            }

            _deterministicRigidbodies = deterministicRigidbodies;
        }

        private void UpdateSimulation() 
        {
            foreach (var keyPairRigidbody in _rigidbodiesComponents)
            {
                var data = keyPairRigidbody.Value.Data.GetMutableData();

                data.Position = (Vector3Fp)keyPairRigidbody.Value.transform.position;
                data.Rotation = (QuaternionFp)keyPairRigidbody.Value.transform.rotation;

                _managedSimulation.UpdateRigidbodyData(keyPairRigidbody.Key, data);
            }

            _managedSimulation.UpdateSimulation(new Fp(Time.fixedDeltaTime));
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;

            foreach (var drb in _deterministicRigidbodies)
            {
                Gizmos.DrawWireCube((Vector3)drb.Value.Bounds.Center, (Vector3)drb.Value.Bounds.Size);
            }
        }
    }
}
