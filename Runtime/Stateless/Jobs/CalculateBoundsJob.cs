using FixedPoint.SubTypes;
using FixedPoint;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;

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
                    data.output.Bounds = new BoundsFp();
                    break;
                case ColliderType.Box:
                    data.output.Bounds = GetBoxBounds(data.input.Position, data.input.Extents, data.input.Rotation);
                    break;
                case ColliderType.Sphere:
                    data.output.Bounds = GetSphereBounds(data.input.Position, data.input.Radius);
                    break;
                default:
                    break;
            }

            rigidbodiesData[index] = data;
        }

        private BoundsFp GetBoxBounds(Vector3Fp center, Vector3Fp extents, QuaternionFp rotation)
        {
            Vector3Fp right = rotation * Vector3Fp.Right;
            Vector3Fp up = rotation * Vector3Fp.Up;
            Vector3Fp forward = rotation * Vector3Fp.Forward;

            Vector3Fp halfExtentsWorld = new Vector3Fp(
                MathFp.Abs(extents.x * right.x) + MathFp.Abs(extents.y * up.x) + MathFp.Abs(extents.z * forward.x),
                MathFp.Abs(extents.x * right.y) + MathFp.Abs(extents.y * up.y) + MathFp.Abs(extents.z * forward.y),
                MathFp.Abs(extents.x * right.z) + MathFp.Abs(extents.y * up.z) + MathFp.Abs(extents.z * forward.z)
            );

            return new BoundsFp(center, halfExtentsWorld);
        }

        private BoundsFp GetSphereBounds(Vector3Fp center, Fp radius)
        {
            return new BoundsFp(center, new Vector3Fp(radius * 2, radius * 2, radius * 2));
        }

        public Vector3Fp TransformWithQuaternion(Vector3Fp vector, QuaternionFp rotation)
        {
            rotation.Normalize();

            Fp x = rotation.x;
            Fp y = rotation.y;
            Fp z = rotation.z;
            Fp w = rotation.w;

            Fp vx = w * vector.x + y * vector.z - z * vector.y;
            Fp vy = w * vector.y + z * vector.x - x * vector.z;
            Fp vz = w * vector.z + x * vector.y - y * vector.x;
            Fp vw = -x * vector.x - y * vector.y - z * vector.z;

            Vector3Fp result;
            result.x = vx * w - vw * x + vy * z - vz * y;
            result.y = vy * w - vw * y + vz * x - vx * z;
            result.z = vz * w - vw * z + vx * y - vy * x;

            return result;
        }
    }
}
