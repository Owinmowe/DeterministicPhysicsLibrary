using FixedPoint;
using FixedPoint.SubTypes;
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace DeterministicPhysicsLibrary.Runtime.Managed
{
    [BurstCompile]
    public struct CollisionDetectionJob : IJobParallelFor
    {
        public NativeArray<DMRigidbodyData> rigidbodiesData;
        public NativeParallelHashSet<int>.ParallelWriter collisionIndexesHashSet;

        public void Execute(int index)
        {
            for (int i = 0; i < rigidbodiesData.Length; i++)
            {
                bool differentCollision = i != index;

                if (!differentCollision)
                    continue;

                bool aabbIntersects = rigidbodiesData[i].simData.Bounds.Intersects(rigidbodiesData[index].simData.Bounds);

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
                        collided = IsCollidingSphereWithBox(i, index);
                    }
                }
                else
                {
                    if (rigidbodiesData[index].input.ColliderType == ColliderType.Sphere)
                    {
                        collided = IsCollidingSphereWithBox(index, i);
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

        private bool IsCollidingSphereWithSphere(DMRigidbodyData sphere1, DMRigidbodyData sphere2)
        {
            Fp sphereCenterDistance = (sphere1.simData.Position - sphere2.simData.Position).Magnitude;
            return sphere1.input.Radius + sphere2.input.Radius >= sphereCenterDistance;
        }

        private bool IsCollidingSphereWithBox(int sphereIndex, int boxIndex)
        {
            DMRigidbodyData box = rigidbodiesData[boxIndex];
            DMRigidbodyData sphere = rigidbodiesData[sphereIndex];

            QuaternionFp inverseRotation = MathQuaternionFp.Inverse(box.simData.Rotation);
            Vector3Fp localSphereCenter = inverseRotation * (sphere.simData.Position - box.simData.Position);

            Vector3Fp localClosestPoint = new Vector3Fp(
                MathFp.Clamp(localSphereCenter.x, -box.input.Extents.x / 2, box.input.Extents.x / 2),
                MathFp.Clamp(localSphereCenter.y, -box.input.Extents.y / 2, box.input.Extents.y / 2),
                MathFp.Clamp(localSphereCenter.z, -box.input.Extents.z / 2, box.input.Extents.z / 2)
            );

            Vector3Fp closestPointWorld = box.simData.Position + (box.simData.Rotation * localClosestPoint);

            box.output.ClosestPointWorld = closestPointWorld;
            sphere.output.ClosestPointWorld = closestPointWorld;

            rigidbodiesData[boxIndex] = box;
            rigidbodiesData[sphereIndex] = sphere;

            Fp distanceSquared = (closestPointWorld - sphere.simData.Position).SqrtMagnitude;
            return distanceSquared <= sphere.input.Radius * sphere.input.Radius;
        }

        private bool IsCollidingBoxWithBox(DMRigidbodyData box1, DMRigidbodyData box2)
        {
            return true;
        }

        private int EncodeCollisionDetectionPair(int index1, int index2)
        {
            int smaller = Math.Min(index1, index2);
            int larger = Math.Max(index1, index2);

            return (smaller << 16) | (larger & 0xFFFF);
        }
    }
}