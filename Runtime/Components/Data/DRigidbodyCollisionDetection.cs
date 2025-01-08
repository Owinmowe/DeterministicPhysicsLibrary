using DeterministicPhysicsLibrary.Runtime;
using FixedPoint;
using FixedPoint.SubTypes;

namespace DeterministicPhysicsLibrary.Unity
{
    [System.Serializable]
    public class DRigidbodyCollisionDetection
    {
        public ColliderType colliderType = ColliderType.None;
        public CollisionLayer collisionLayer = CollisionLayer.Layer1;

        public Fp radius = new Fp(.5f);
        public Vector3Fp extents = Vector3Fp.One;
    }
}