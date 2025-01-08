using System;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using FixedPoint;
using UnityEngine;
using FixedPoint.SubTypes;

namespace DeterministicPhysicsLibrary.Runtime.Stateless
{
    [BurstCompile]
    public struct CollisionDetectionJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<DSRigidbodyData> rigidbodiesData;
        public NativeParallelHashSet<int>.ParallelWriter collisionIndexesHashSet;

        public void Execute(int index)
        {
            for (int i = 0; i < rigidbodiesData.Length; i++)
            {
                bool differentCollision = i != index;

                if (!differentCollision)
                    continue;

                bool aabbIntersects = rigidbodiesData[i].output.Bounds.Intersects(rigidbodiesData[index].output.Bounds);

                if (!aabbIntersects)
                    continue;

                bool layersCollided = SameLayerCollision(rigidbodiesData[i].input.CollisionLayer, rigidbodiesData[index].input.CollisionLayer);

                if (!layersCollided)
                    continue;

                bool collided = true;

                if (rigidbodiesData[i].input.ColliderType == ColliderType.Sphere)
                {
                    if (rigidbodiesData[index].input.ColliderType == ColliderType.Sphere)
                    {
                        collided = IsCollidingSphereWithSphere(rigidbodiesData[i], rigidbodiesData[index]);
                    }
                    else
                    {
                        collided = IsCollidingSphereWithBox(rigidbodiesData[i], rigidbodiesData[index]);
                    }
                }
                else
                {
                    if (rigidbodiesData[index].input.ColliderType == ColliderType.Sphere)
                    {
                        collided = IsCollidingSphereWithBox(rigidbodiesData[i], rigidbodiesData[index]);
                    }
                    else
                    {
                        collided = IsCollidingBoxWithBox(rigidbodiesData[i], rigidbodiesData[index]);
                    }
                }

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

        private bool IsCollidingSphereWithSphere(DSRigidbodyData sphere1, DSRigidbodyData sphere2)
        {
            Fp sphereCenterDistance = (sphere1.input.Position - sphere2.output.PredictedPosition).Magnitude;
            return sphere1.input.Radius + sphere2.input.Radius >= sphereCenterDistance;
        }

        private bool IsCollidingSphereWithBox(DSRigidbodyData sphere, DSRigidbodyData box)
        {
            return true;
        }

        private bool IsCollidingBoxWithBox(DSRigidbodyData box1, DSRigidbodyData box2)
        {
            return true;
        }

        private int EncodeCollisionDetectionPair(int index1, int index2)
        {
            int min = Math.Min(index1, index2);
            int max = Math.Max(index1, index2);

            return (min << 16) | (max & 0xFFFF);
        }
    }
}