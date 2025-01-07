using UnityEngine;
using FixedPoint.SubTypes;

namespace DeterministicPhysicsLibrary.Unity
{
    [System.Serializable]
    public class DMRigidbodySimulationData
    {
        [HideInInspector] public ulong rigidBodyIndex;
        [HideInInspector] public Vector3Fp acceleration;
        [HideInInspector] public Vector3Fp angularAcceleration;
    }
}