using FixedPoint;
using FixedPoint.SubTypes;

namespace DeterministicPhysicsLibrary.Unity
{
    [System.Serializable]
    public class DRigidbodyDynamics
    {
        public Vector3Fp gravity = new Vector3Fp(0, -9.81f, 0);
        public Fp mass = 1;
    }
}