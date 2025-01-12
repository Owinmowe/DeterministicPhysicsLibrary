using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;

namespace DeterministicPhysicsLibrary.Runtime.Stateless
{
    [BurstCompile]
    public struct DynamicsJob : IJobParallelFor
    {
        public NativeArray<DSRigidbodyData> rigidbodiesData;
        public float deltaTime;

        public void Execute(int index)
        {
            DSRigidbodyData rigidbody = rigidbodiesData[index];

            rigidbody.output.Colliding = false;
            rigidbody.output.UsePredictedPosition = false;
            rigidbody.output.UsePredictedRotation = false;

            rigidbody.output.Velocity += rigidbody.input.ExternalAcceleration * deltaTime;
            rigidbody.output.AngularVelocity += rigidbody.input.ExternalAngularAcceleration * deltaTime;

            rigidbody.output.PredictedPosition = rigidbody.input.Position + rigidbody.output.Velocity * deltaTime;
            rigidbody.output.PredictedRotation = rigidbody.input.Rotation;
            //rigidbody.output.PredictedRotation = rigidbody.input.PredictedRotation + rigidbody.output.AngularVelocity * deltaTime;

            rigidbodiesData[index] = rigidbody;
        }
    }
}