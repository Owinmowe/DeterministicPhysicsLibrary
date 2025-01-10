using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace DeterministicPhysicsLibrary.Runtime.Managed
{
    [BurstCompile]
    public struct CollisionResolutionJob : IJob
    {
        [ReadOnly] public NativeArray<int> collisionIndexesArray;
        public NativeArray<DMRigidbodyData> rigidbodiesData;

        public void Execute()
        {
            for (int i = 0; i < collisionIndexesArray.Length; i++)
            {
                DecodeCollisionDetectionPair(collisionIndexesArray[i], out int colliderIndexA, out int colliderIndexB);

                DMRigidbodyData dataA = rigidbodiesData[colliderIndexA];
                DMRigidbodyData dataB = rigidbodiesData[colliderIndexB];

                dataA.output.Colliding = true;
                dataB.output.Colliding = true;

                rigidbodiesData[colliderIndexA] = dataA;
                rigidbodiesData[colliderIndexB] = dataB;
            }
        }

        private void DecodeCollisionDetectionPair(int encoded, out int index1, out int index2)
        {
            index1 = encoded >> 16;
            index2 = encoded & 0xFFFF;
        }
    }
}