using UnityEngine;
using FixedPoint.SubTypes;

namespace DeterministicPhysicsLibrary.Unity
{
    [System.Serializable]
    public class DRigidbodySimulationData
    {
        [HideInInspector] public int rigidBodyIndex;
        [HideInInspector] public Vector3Fp externalAcceleration;
        [HideInInspector] public Vector3Fp externalAngularAcceleration;
    }
}