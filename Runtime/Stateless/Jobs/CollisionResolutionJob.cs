using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;

namespace DeterministicPhysicsLibrary.Runtime.Stateless
{
    [BurstCompile]
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

                dataA.output.Colliding = true;
                dataB.output.Colliding = true;

                bool aKinematic = IsKinematic(dataA.input.CollisionResponseType);
                bool bKinematic = IsKinematic(dataB.input.CollisionResponseType);

                if (aKinematic && bKinematic) 
                {
                    dataA.output.PredictedPosition += dataA.output.PenetrationDepthVector;
                    dataA.output.Velocity *= -1;
                    
                    dataA.output.UsePredictedPosition = true;
                    dataA.output.UsePredictedRotation = true;

                    dataB.output.PredictedPosition += dataB.output.PenetrationDepthVector;
                    dataB.output.Velocity *= -1;

                    dataB.output.UsePredictedPosition = true;
                    dataB.output.UsePredictedRotation = true;
                }
                else if (aKinematic)
                {
                    dataA.output.PredictedPosition += dataA.output.PenetrationDepthVector * 2;
                    Vector3 finalVelocity = dataA.output.Velocity.magnitude * dataA.output.PenetrationDepthVector * 2;
                    dataA.output.Velocity = finalVelocity;

                    dataA.output.UsePredictedPosition = true;
                    dataA.output.UsePredictedRotation = true;
                }
                else
                {
                    dataB.output.PredictedPosition += dataB.output.PenetrationDepthVector * 2;
                    Vector3 finalVelocity = dataB.output.Velocity.magnitude * dataB.output.PenetrationDepthVector * 2;
                    dataB.output.Velocity = finalVelocity;

                    dataB.output.UsePredictedPosition = true;
                    dataB.output.UsePredictedRotation = true;
                }

                rigidbodiesData[colliderIndexA] = dataA;
                rigidbodiesData[colliderIndexB] = dataB;
            }
        }

        private void DecodeCollisionDetectionPair(int encoded, out int index1, out int index2)
        {
            index1 = encoded >> 16;
            index2 = encoded & 0xFFFF;
        }

        private bool IsKinematic(CollisionResponseType responseType)
        {
            return (responseType & CollisionResponseType.Kinematic) != 0;
        }
    }
}