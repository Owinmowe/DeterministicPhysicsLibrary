using System;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;

namespace DeterministicPhysicsLibrary.Runtime.Stateless
{
    [BurstCompile]
    public struct CollisionDetectionJob : IJobParallelFor
    {
        public NativeArray<DSRigidbodyData> rigidbodiesData;
        public NativeParallelHashSet<int>.ParallelWriter collisionIndexesHashSet;

        public void Execute(int index)
        {
            ColliderType typeBody1 = rigidbodiesData[index].input.ColliderType;
            
            if (typeBody1 == ColliderType.None)
                return;

            for (int i = 0; i < rigidbodiesData.Length; i++)
            {
                bool differentCollision = i != index;

                if (!differentCollision)
                    continue;

                ColliderType typeBody2 = rigidbodiesData[i].input.ColliderType;
                if (typeBody2 == ColliderType.None)
                    return;

                bool aabbIntersects = rigidbodiesData[i].output.Bounds.Intersects(rigidbodiesData[index].output.Bounds);

                if (!aabbIntersects)
                    continue;

                bool layersCollided = SameLayerCollision(rigidbodiesData[i].input.CollisionLayer, rigidbodiesData[index].input.CollisionLayer);

                if (!layersCollided)
                    continue;

                bool collided = true;


                if (typeBody1 == ColliderType.Box)
                {
                    if (typeBody2 == ColliderType.Box) 
                    {
                        collided = IsCollidingBoxWithBox(rigidbodiesData[i], rigidbodiesData[index]);
                    }
                    else 
                    {
                        collided = IsCollidingSphereWithBox(i, index);
                    }

                }
                else// if (typeBody1 == ColliderType.Sphere)
                {
                    if (typeBody2 == ColliderType.Box)
                    {
                        collided = IsCollidingSphereWithBox(index, i);
                    }
                    else
                    {
                        collided = IsCollidingSphereWithSphere(rigidbodiesData[index], rigidbodiesData[i]);
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
            float sphereCenterDistance = (sphere1.input.Position - sphere2.output.PredictedPosition).magnitude;
            return sphere1.input.Radius + sphere2.input.Radius >= sphereCenterDistance;
        }

        private bool IsCollidingSphereWithBox(int sphereIndex,int boxIndex) 
        {
            DSRigidbodyData box = rigidbodiesData[boxIndex];
            DSRigidbodyData sphere = rigidbodiesData[sphereIndex];

            Quaternion inverseRotation = Quaternion.Inverse(box.output.PredictedRotation);
            Vector3 localSphereCenter = inverseRotation * (sphere.output.PredictedPosition - box.output.PredictedPosition);

            Vector3 localClosestPoint = new Vector3(
                Mathf.Clamp(localSphereCenter.x, -box.input.Extents.x / 2, box.input.Extents.x / 2),
                Mathf.Clamp(localSphereCenter.y, -box.input.Extents.y / 2, box.input.Extents.y / 2),
                Mathf.Clamp(localSphereCenter.z, -box.input.Extents.z / 2, box.input.Extents.z / 2)
            );

            Vector3 closestPointWorld = box.output.PredictedPosition + (box.output.PredictedRotation * localClosestPoint);

            box.output.ClosestPointWorld = closestPointWorld;
            sphere.output.ClosestPointWorld = closestPointWorld;

            rigidbodiesData[boxIndex] = box;
            rigidbodiesData[sphereIndex] = sphere;

            float distanceSquared = (closestPointWorld - sphere.output.PredictedPosition).sqrMagnitude;
            return distanceSquared <= sphere.input.Radius * sphere.input.Radius;
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