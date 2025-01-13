using System.Collections.Generic;
using UnityEngine;
using DeterministicPhysicsLibrary.Runtime.Stateless;

namespace DeterministicPhysicsLibrary.Unity
{
    public static class DSPhysicsSystem
    {
        private static DSSimulation _statelessSimulation;
        private static readonly Dictionary<int, DSRigidbody> _rigidbodiesComponents = new();

        public static void Initialize() 
        {
            if (_statelessSimulation != null) 
            {
                _statelessSimulation.RigidbodyUpdateCompleteEvent -= ReceiveRigidbodyDataFromSimulation;
                _statelessSimulation = null;
            }

            _statelessSimulation = new DSSimulation();
            _statelessSimulation.RigidbodyUpdateCompleteEvent += ReceiveRigidbodyDataFromSimulation;
        }

        public static void UpdateSimulation()
        {
            foreach (var keyPairRigidbody in _rigidbodiesComponents)
            {
                var data = keyPairRigidbody.Value.Data.GetInputData();

                data.Position = keyPairRigidbody.Value.transform.position;
                data.Rotation = keyPairRigidbody.Value.transform.rotation;

                _statelessSimulation.UpdateRigidbodyData(keyPairRigidbody.Key, data);
            }

            _statelessSimulation.UpdateSimulation(Time.fixedDeltaTime);
        }

        public static int AddRigidbody(DSRigidbody rigidBodyComponent)
        {
            DSRigidbodyInputData data = rigidBodyComponent.Data.GetInputData();

            data.Position = rigidBodyComponent.transform.position;
            data.Rotation = rigidBodyComponent.transform.rotation;

            int rigidbodyIndex = _statelessSimulation.AddRigidbody(data);

            _rigidbodiesComponents.Add(rigidbodyIndex, rigidBodyComponent);
            return rigidbodyIndex;
        }

        public static void RemoveRigidbody(int rigidbodyIndex)
        {
            _statelessSimulation.RemoveRigidbody(rigidbodyIndex);
            _rigidbodiesComponents.Remove(rigidbodyIndex);
        }

        private static void ReceiveRigidbodyDataFromSimulation(Dictionary<int, Runtime.Stateless.DSRigidbodyData> deterministicRigidbodies)
        {
            foreach (var rigidBody in deterministicRigidbodies)
            {
                if (rigidBody.Value.output.UsePredictedPosition) 
                {
                    _rigidbodiesComponents[rigidBody.Key].transform.position = rigidBody.Value.output.PredictedPosition;
                }

                Vector3 positionUpdate = (rigidBody.Value.output.Velocity);
                _rigidbodiesComponents[rigidBody.Key].transform.position += positionUpdate;

                if (rigidBody.Value.output.UsePredictedRotation)
                {
                    _rigidbodiesComponents[rigidBody.Key].transform.rotation = rigidBody.Value.output.PredictedRotation;
                }

                Vector3 rotationUpdate = (rigidBody.Value.output.AngularVelocity);
                _rigidbodiesComponents[rigidBody.Key].transform.rotation *= Quaternion.Euler(rotationUpdate);
            }
        }
    }
}