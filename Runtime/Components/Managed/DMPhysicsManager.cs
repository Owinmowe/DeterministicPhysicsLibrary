using System.Collections.Generic;
using UnityEngine;
using FixedPoint;
using FixedPoint.SubTypes;
using DeterministicPhysicsLibrary.Runtime;
using DeterministicPhysicsLibrary.Runtime.Managed;

namespace DeterministicPhysicsLibrary.Unity
{
    public class DMPhysicsManager : MonoBehaviour
    {
        private static DMSimulation _managedSimulation;
        private static readonly Dictionary<int, DMRigidbody> _rigidbodiesComponents = new();

        Dictionary<int, Runtime.Managed.DMRigidbodyData> _gizmosDictionary = new();

        [RuntimeInitializeOnLoadMethod(loadType: RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnLoadScene()
        {
            DMRigidbody.OnRigidbodyEnabled += AddRigidbody;
            DMRigidbody.OnRigidbodyDisabled += RemoveRigidbody;

            _managedSimulation = new DMSimulation();
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

        private static int AddRigidbody(DMRigidbody rigidBodyComponent)
        {
            DMRigidbodyInputData data = rigidBodyComponent.Data.GetInputData();

            Vector3Fp position = (Vector3Fp)rigidBodyComponent.transform.position;
            QuaternionFp rotation = (QuaternionFp)rigidBodyComponent.transform.rotation;

            int rigidbodyIndex = _managedSimulation.AddRigidbody(data, position, rotation);
            
            _rigidbodiesComponents.Add(rigidbodyIndex, rigidBodyComponent);
            return rigidbodyIndex;
        }

        private static void RemoveRigidbody(int rigidbodyIndex)
        {
            _managedSimulation.RemoveRigidbody(rigidbodyIndex);
            _rigidbodiesComponents.Remove(rigidbodyIndex);
        }

        private void ReceiveRigidbodyDataFromSimulation(Dictionary<int, Runtime.Managed.DMRigidbodyData> deterministicRigidbodies)
        {
            foreach (var rigidBody in deterministicRigidbodies)
            {
                _rigidbodiesComponents[rigidBody.Key].transform.position = (Vector3)rigidBody.Value.output.Position;
                _rigidbodiesComponents[rigidBody.Key].transform.rotation = (Quaternion)rigidBody.Value.output.Rotation;
            }

            _gizmosDictionary = deterministicRigidbodies;
        }

        private void UpdateSimulation() 
        {
            foreach (var keyPairRigidbody in _rigidbodiesComponents)
            {
                var data = keyPairRigidbody.Value.Data.GetInputData();
                _managedSimulation.UpdateRigidbodyData(keyPairRigidbody.Key, data);
            }

            _managedSimulation.UpdateSimulation(new Fp(Time.fixedDeltaTime));
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;

            foreach (var drb in _gizmosDictionary)
            {
                Gizmos.DrawWireCube((Vector3)drb.Value.output.Bounds.Center, (Vector3)drb.Value.output.Bounds.Size);
            }
        }
    }
}
