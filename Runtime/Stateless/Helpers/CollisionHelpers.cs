using System.Runtime.CompilerServices;
using UnityEngine;

namespace DeterministicPhysicsLibrary.Runtime.Stateless
{
    public static class CollisionHelpers
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsColliding(ref DSRigidbodyData rigidbodyA, ColliderType rigidbodyAType, ref DSRigidbodyData rigidbodyB, ColliderType rigidbodyBType)
        {
            if (rigidbodyAType == ColliderType.Box)
            {
                if (rigidbodyBType == ColliderType.Box)
                {
                    return IsCollidingBoxWithBox(ref rigidbodyB, ref rigidbodyA);
                }
                else
                {
                    return IsCollidingSphereWithBox(ref rigidbodyB, ref rigidbodyA);
                }

            }
            else// if (rigidbodyAType == ColliderType.Sphere)
            {
                if (rigidbodyBType == ColliderType.Box)
                {
                    return IsCollidingSphereWithBox(ref rigidbodyA, ref rigidbodyB);
                }
                else
                {
                    return IsCollidingSphereWithSphere(ref rigidbodyA, ref rigidbodyB);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCollidingSphereWithSphere(ref DSRigidbodyData sphere1, ref DSRigidbodyData sphere2)
        {
            float radiusSum = sphere1.input.Radius + sphere2.input.Radius;
            Vector3 centerBetweenSpheres = (sphere2.output.PredictedPosition - sphere1.output.PredictedPosition);

            float penetrationDepth = radiusSum - centerBetweenSpheres.magnitude;

            sphere1.output.PenetrationDepthVector = -centerBetweenSpheres.normalized * penetrationDepth;
            sphere2.output.PenetrationDepthVector = centerBetweenSpheres.normalized * penetrationDepth;

            return radiusSum >= centerBetweenSpheres.magnitude;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCollidingSphereWithBox(ref DSRigidbodyData sphere, ref DSRigidbodyData box)
        {
            Quaternion inverseRotation = Quaternion.Inverse(box.output.PredictedRotation);
            Vector3 localSphereCenter = inverseRotation * (sphere.output.PredictedPosition - box.output.PredictedPosition);

            Vector3 localClosestPoint = new Vector3(
                Mathf.Clamp(localSphereCenter.x, -box.input.Extents.x / 2, box.input.Extents.x / 2),
                Mathf.Clamp(localSphereCenter.y, -box.input.Extents.y / 2, box.input.Extents.y / 2),
                Mathf.Clamp(localSphereCenter.z, -box.input.Extents.z / 2, box.input.Extents.z / 2)
            );

            Vector3 closestPointWorld = box.output.PredictedPosition + (box.output.PredictedRotation * localClosestPoint);

            float distanceSquared = (closestPointWorld - sphere.output.PredictedPosition).sqrMagnitude;
            return distanceSquared <= sphere.input.Radius * sphere.input.Radius;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCollidingBoxWithBox(ref DSRigidbodyData box1, ref DSRigidbodyData box2)
        {
            return true;
        }
    }
}