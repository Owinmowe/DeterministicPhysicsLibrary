using System.Collections.Generic;
using UnityEngine;
using FixedPoint;
using FixedPoint.SubTypes;
using DeterministicPhysicsLibrary.Runtime.Managed;
using DeterministicPhysicsLibrary.Runtime.Stateless;

namespace DeterministicPhysicsLibrary.Unity
{
    public class DMPhysicsSystem
    {
        private static DMSimulation _managedSimulation;
        private static readonly Dictionary<int, DMRigidbody> _rigidbodiesComponents = new();

        public static void Initialize() 
        {
            if (_managedSimulation != null)
            {
                _managedSimulation.RigidbodyUpdateCompleteEvent -= ReceiveRigidbodyDataFromSimulation;
                _managedSimulation = null;
            }

            _managedSimulation = new DMSimulation();
            _managedSimulation.RigidbodyUpdateCompleteEvent += ReceiveRigidbodyDataFromSimulation;
        }

        public static void UpdateSimulation()
        {
            foreach (var keyPairRigidbody in _rigidbodiesComponents)
            {
                var data = keyPairRigidbody.Value.Data.GetInputData();
                _managedSimulation.UpdateRigidbodyData(keyPairRigidbody.Key, data);
            }

            _managedSimulation.UpdateSimulation(new Fp(Time.fixedDeltaTime));
        }

        public static int AddRigidbody(DMRigidbody rigidBodyComponent)
        {
            DMRigidbodyInputData data = rigidBodyComponent.Data.GetInputData();

            Vector3Fp position = (Vector3Fp)rigidBodyComponent.transform.position;
            QuaternionFp rotation = (QuaternionFp)rigidBodyComponent.transform.rotation;

            int rigidbodyIndex = _managedSimulation.AddRigidbody(data, position, rotation);
            
            _rigidbodiesComponents.Add(rigidbodyIndex, rigidBodyComponent);
            return rigidbodyIndex;
        }

        public static void RemoveRigidbody(int rigidbodyIndex)
        {
            _managedSimulation.RemoveRigidbody(rigidbodyIndex);
            _rigidbodiesComponents.Remove(rigidbodyIndex);
        }

        private static void ReceiveRigidbodyDataFromSimulation(Dictionary<int, Runtime.Managed.DMRigidbodyData> deterministicRigidbodies)
        {
            foreach (var rigidBody in deterministicRigidbodies)
            {
                _rigidbodiesComponents[rigidBody.Key].transform.position = (Vector3)rigidBody.Value.output.Position;
                _rigidbodiesComponents[rigidBody.Key].transform.rotation = (Quaternion)rigidBody.Value.output.Rotation;
            }
        }
    }
}
