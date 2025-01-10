using System.Collections.Generic;
using UnityEngine;
using DeterministicPhysicsLibrary.Runtime.Stateless;

namespace DeterministicPhysicsLibrary.Unity
{
    public class DSPhysicsManager : MonoBehaviour
    {
        private static DSSimulation _statelessSimulation;
        private static readonly Dictionary<int, DSRigidbody> _rigidbodiesComponents = new();

        Dictionary<int, Runtime.Stateless.DSRigidbodyData> _gizmosDictionary = new();

        [RuntimeInitializeOnLoadMethod(loadType: RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnLoadScene()
        {
            DSRigidbody.OnRigidbodyEnabled += AddRigidbody;
            DSRigidbody.OnRigidbodyDisabled += RemoveRigidbody;

            _statelessSimulation = new DSSimulation();
        }

        private void Awake()
        {
            _statelessSimulation.RigidbodyUpdateCompleteEvent += ReceiveRigidbodyDataFromSimulation;
        }

        private void FixedUpdate()
        {
            UpdateSimulation();
        }

        private void OnDestroy()
        {
            _statelessSimulation.RigidbodyUpdateCompleteEvent -= ReceiveRigidbodyDataFromSimulation;
            _statelessSimulation = null;

            DSRigidbody.OnRigidbodyEnabled -= AddRigidbody;
            DSRigidbody.OnRigidbodyDisabled -= RemoveRigidbody;
        }

        private static int AddRigidbody(DSRigidbody rigidBodyComponent)
        {
            DSRigidbodyInputData data = rigidBodyComponent.Data.GetInputData();

            data.Position = rigidBodyComponent.transform.position;
            data.Rotation = rigidBodyComponent.transform.rotation;

            int rigidbodyIndex = _statelessSimulation.AddRigidbody(data);

            _rigidbodiesComponents.Add(rigidbodyIndex, rigidBodyComponent);
            return rigidbodyIndex;
        }

        private static void RemoveRigidbody(int rigidbodyIndex)
        {
            _statelessSimulation.RemoveRigidbody(rigidbodyIndex);
            _rigidbodiesComponents.Remove(rigidbodyIndex);
        }

        private void ReceiveRigidbodyDataFromSimulation(Dictionary<int, Runtime.Stateless.DSRigidbodyData> deterministicRigidbodies)
        {
            foreach (var rigidBody in deterministicRigidbodies)
            {
                Vector3 positionUpdate = (Vector3)(rigidBody.Value.output.Velocity) * Time.fixedDeltaTime;
                _rigidbodiesComponents[rigidBody.Key].transform.position += positionUpdate;

                Vector3 rotationUpdate = (Vector3)(rigidBody.Value.output.AngularVelocity) * Time.fixedDeltaTime;
                _rigidbodiesComponents[rigidBody.Key].transform.rotation *= Quaternion.Euler(rotationUpdate);
            }

            _gizmosDictionary = deterministicRigidbodies;
        }

        private void UpdateSimulation()
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

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;

            foreach (var drb in _gizmosDictionary)
            {
                Gizmos.color = drb.Value.output.Colliding ? Color.red : Color.green;

                Gizmos.DrawWireCube(drb.Value.output.Bounds.center, drb.Value.output.Bounds.size);

                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(drb.Value.output.ClosestPointWorld, .1f);
            }
        }
    }
}