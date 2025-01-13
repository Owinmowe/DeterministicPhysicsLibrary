using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;

namespace DeterministicPhysicsLibrary.Runtime.Stateless
{
    //[BurstCompile]
    public struct CollisionResolutionJob : IJob
    {
        [ReadOnly] public NativeArray<int> collisionIndexesArray;
        public NativeArray<DSRigidbodyData> rigidbodiesData;

        public void Execute()
        {
            for (int i = 0; i < collisionIndexesArray.Length; i++)
            {
                DecodeCollisionDetectionPair(collisionIndexesArray[i], out int colliderIndexA, out int colliderIndexB);

                DSRigidbodyData dataA = rigidbodiesData[colliderIndexA];
                DSRigidbodyData dataB = rigidbodiesData[colliderIndexB];

                CalculateKinematicCollisions(ref dataA, ref dataB);

                dataA.output.Colliding = true;
                dataB.output.Colliding = true;

                rigidbodiesData[colliderIndexA] = dataA;
                rigidbodiesData[colliderIndexB] = dataB;
            }
        }

        private void CalculateKinematicCollisions(ref DSRigidbodyData dataA, ref DSRigidbodyData dataB) 
        {
            bool aKinematic = IsKinematic(dataA.input.CollisionResponseType);
            bool bKinematic = IsKinematic(dataB.input.CollisionResponseType);

            if (!aKinematic && !bKinematic)
                return;

            if (aKinematic && bKinematic)
            {
                ResolveCollision(ref dataA, dataA.output.PenetrationDepthVector / 2);
                ResolveCollision(ref dataB, dataB.output.PenetrationDepthVector / 2);
            }
            else if (aKinematic)
            {
                ResolveCollision(ref dataA, dataA.output.PenetrationDepthVector);
            }
            else
            {
                ResolveCollision(ref dataB, dataB.output.PenetrationDepthVector);
            }
        }

        private void ResolveCollision(ref DSRigidbodyData data, Vector3 penetrationVector) 
        {
            data.output.PredictedPosition += penetrationVector;
            Vector3 finalVelocity = data.output.PenetrationDepthVector * data.output.Velocity.magnitude;
            data.output.Velocity = finalVelocity;

            data.output.UsePredictedPosition = true;
            data.output.UsePredictedRotation = true;
        }

        private bool IsKinematic(CollisionResponseType responseType)
        {
            return CollisionResponseUtils.HasFlag(responseType, CollisionResponseType.Kinematic);
        }

        private void DecodeCollisionDetectionPair(int encoded, out int index1, out int index2)
        {
            index1 = encoded >> 16;
            index2 = encoded & 0xFFFF;
        }
    }
}