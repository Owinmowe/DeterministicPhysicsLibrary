using FixedPoint;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;

namespace DeterministicPhysicsLibrary.Runtime.Managed
{
    [BurstCompile]
    public struct DynamicsJob : IJobParallelFor
    {
        public NativeArray<DMRigidbodyData> rigidbodiesData;
        public Fp deltaTime;

        public void Execute(int index)
        {
            DMRigidbodyData data = rigidbodiesData[index];

            data.simData.Acceleration += data.input.ExternalAcceleration * deltaTime;
            data.simData.AngularAcceleration += data.input.ExternalAngularAcceleration * deltaTime;

            data.simData.Velocity += data.simData.Acceleration * deltaTime;            
            data.simData.AngularVelocity += data.simData.AngularAcceleration * deltaTime;

            data.simData.Position += data.simData.Velocity * deltaTime;
            //data.simData.Rotation *= data.input.AngularAcceleration * deltaTime;

            data.output.Position = data.simData.Position;
            data.output.Rotation = data.simData.Rotation;
            data.output.Colliding = false;

            rigidbodiesData[index] = data;
        }
    }
}