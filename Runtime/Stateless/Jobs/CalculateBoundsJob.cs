using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine;

namespace DeterministicPhysicsLibrary.Runtime.Stateless 
{
    [BurstCompile]
    public struct CalculateBoundsJob : IJobParallelFor
    {
        public NativeArray<DSRigidbodyData> rigidbodiesData;

        public void Execute(int index)
        {
            DSRigidbodyData data = rigidbodiesData[index];

            switch (data.input.ColliderType)
            {
                case ColliderType.None:
                    data.output.Bounds = new Bounds();
                    break;
                case ColliderType.Box:
                    data.output.Bounds = GetBoxBounds(data.output.PredictedPosition, data.input.Extents, data.input.Rotation);
                    break;
                case ColliderType.Sphere:
                    data.output.Bounds = GetSphereBounds(data.output.PredictedPosition, data.input.Radius);
                    break;
                default:
                    break;
            }

            rigidbodiesData[index] = data;
        }

        private Bounds GetBoxBounds(Vector3 center, Vector3 extents, Quaternion rotation)
        {
            Vector3 right = rotation * Vector3.right;
            Vector3 up = rotation * Vector3.up;
            Vector3 forward = rotation * Vector3.forward;

            Vector3 halfExtentsWorld = new Vector3(
                Mathf.Abs(extents.x * right.x) + Mathf.Abs(extents.y * up.x) + Mathf.Abs(extents.z * forward.x),
                Mathf.Abs(extents.x * right.y) + Mathf.Abs(extents.y * up.y) + Mathf.Abs(extents.z * forward.y),
                Mathf.Abs(extents.x * right.z) + Mathf.Abs(extents.y * up.z) + Mathf.Abs(extents.z * forward.z)
            );

            return new Bounds(center, halfExtentsWorld);
        }

        private Bounds GetSphereBounds(Vector3 center, float radius)
        {
            return new Bounds(center, new Vector3(radius * 2, radius * 2, radius * 2));
        }

        public Vector3 TransformWithQuaternion(Vector3 vector, Quaternion rotation)
        {
            rotation.Normalize();

            float x = rotation.x;
            float y = rotation.y;
            float z = rotation.z;
            float w = rotation.w;

            float vx = w * vector.x + y * vector.z - z * vector.y;
            float vy = w * vector.y + z * vector.x - x * vector.z;
            float vz = w * vector.z + x * vector.y - y * vector.x;
            float vw = -x * vector.x - y * vector.y - z * vector.z;

            Vector3 result;
            result.x = vx * w - vw * x + vy * z - vz * y;
            result.y = vy * w - vw * y + vz * x - vx * z;
            result.z = vz * w - vw * z + vx * y - vy * x;

            return result;
        }
    }
}
