using System;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;

namespace DeterministicPhysicsLibrary.Runtime.Stateless
{
    //[BurstCompile]
    public struct CollisionDetectionJob : IJobParallelFor
    {
        public NativeArray<DSRigidbodyData> rigidbodiesData;
        public NativeParallelHashSet<int>.ParallelWriter collisionIndexesHashSet;

        public void Execute(int index)
        {
            ColliderType typeBodyIndex = rigidbodiesData[index].input.ColliderType;
            
            if (typeBodyIndex == ColliderType.None)
                return;

            for (int i = 0; i < rigidbodiesData.Length; i++)
            {
                bool differentCollision = i != index;

                if (!differentCollision)
                    continue;

                ColliderType typeBodyI = rigidbodiesData[i].input.ColliderType;
                if (typeBodyI == ColliderType.None)
                    return;

                bool aabbIntersects = rigidbodiesData[i].output.Bounds.Intersects(rigidbodiesData[index].output.Bounds);

                if (!aabbIntersects)
                    continue;

                bool layersCollided = SameLayerCollision(rigidbodiesData[i].input.CollisionLayer, rigidbodiesData[index].input.CollisionLayer);

                if (!layersCollided)
                    continue;

                bool collided = true;

                DSRigidbodyData rigidbodyI = rigidbodiesData[i];
                DSRigidbodyData rigidbodyIndex = rigidbodiesData[index];

                collided = CollisionHelpers.IsColliding(ref rigidbodyI, typeBodyI, ref rigidbodyIndex, typeBodyIndex);

                rigidbodiesData[i] = rigidbodyI;
                rigidbodiesData[index] = rigidbodyIndex;

                if (collided)
                {
                    int encodedPair = EncodeCollisionDetectionPair(index, i);
                    collisionIndexesHashSet.Add(encodedPair);
                }
            }
        }

        private bool SameLayerCollision(CollisionLayer layers1, CollisionLayer layers2)
        {
            return (layers1 & layers2) != 0;
        }

        private int EncodeCollisionDetectionPair(int index1, int index2)
        {
            int min = Math.Min(index1, index2);
            int max = Math.Max(index1, index2);

            return (min << 16) | (max & 0xFFFF);
        }
    }
}