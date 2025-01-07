using System;

namespace DeterministicPhysicsLibrary.Runtime
{
    public struct CollisionIndexData : IEquatable<CollisionIndexData>
    {
        public int indexCollided;
        public int indexCollidedWith;

        public bool Equals(CollisionIndexData other)
        {
            bool collisionDirect = indexCollided == other.indexCollided && indexCollidedWith == other.indexCollidedWith;
            bool collisionReverse = indexCollided == other.indexCollidedWith && indexCollidedWith == other.indexCollided;
            return collisionDirect || collisionReverse;
        }
    }
}