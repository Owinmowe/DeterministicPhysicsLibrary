using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using FixedPoint;
using FixedPoint.SubTypes;
using DeterministicPhysicsLibrary.Runtime;

namespace DeterministicPhysicsLibrary
{
    public class ManagedDeterministicSimulation
    {
        public event Action<Dictionary<ulong, DeterministicRigidbodyData>> RigidbodyUpdateCompleteEvent;
        
        private Dictionary<ulong, DeterministicRigidbodyData> _rigidbodiesDictionary;
        private ulong _rigidbodyIndex;

        NativeArray<DeterministicRigidbodyData> _rigidbodiesArray;
        NativeParallelHashSet<int> _collisionIndexesHashSet;

        public ManagedDeterministicSimulation()
        {
            _rigidbodiesDictionary = new();
            _rigidbodiesArray = new(_rigidbodiesDictionary.Values.ToArray(), Allocator.Persistent);
            _collisionIndexesHashSet = new(_rigidbodiesDictionary.Count * _rigidbodiesDictionary.Count, Allocator.Persistent);
        }

        ~ManagedDeterministicSimulation() 
        {
            DisposeCurrentNativeArrays();
        }

        public ulong AddRigidbody(DeterministicRigidbodyData rigidbody)
        {
            rigidbody.Index = _rigidbodyIndex;
            _rigidbodiesDictionary.Add(_rigidbodyIndex, rigidbody);

            DisposeCurrentNativeArrays();
            CreateCurrentNativeArrays();

            _rigidbodyIndex++;
            return rigidbody.Index;
        }

        public void UpdateRigidbodyData(ulong rigidbodyIndex, DeterministicRigidbodyData rigidbodyData) 
        {
            DeterministicRigidbodyData data = _rigidbodiesDictionary[rigidbodyIndex];
            data.SetMutableData(rigidbodyData);
            _rigidbodiesDictionary[rigidbodyIndex] = data;
            _rigidbodiesArray[(int)rigidbodyIndex] = data;

        }

        public void RemoveRigidbody(ulong rigidbodyIndex)
        {
            _rigidbodiesDictionary.Remove(rigidbodyIndex);

            DisposeCurrentNativeArrays();
            CreateCurrentNativeArrays();
        }
        
        public void UpdateSimulation(Fp deltaTime)
        {
            if (!_rigidbodiesArray.IsCreated || !_collisionIndexesHashSet.IsCreated)
                return;

            var dynamicsJob = new DynamicsJob()
            {
                rigidbodiesArray = _rigidbodiesArray,
                deltaTime = deltaTime
            };

            JobHandle dynamicsJobHandle = dynamicsJob.Schedule(_rigidbodiesArray.Length, 64);
            dynamicsJobHandle.Complete();

            var calculateBoundsJob = new CalculateBoundsJob()
            {
                rigidbodiesArray = _rigidbodiesArray
            };
            JobHandle calculateBoundsJobHandle = calculateBoundsJob.Schedule(_rigidbodiesArray.Length, 64);
            calculateBoundsJobHandle.Complete();

            var collisionDetectionJob = new CollisionDetectionJob()
            {
                rigidbodiesArray = _rigidbodiesArray,
                collisionIndexesHashSet = _collisionIndexesHashSet.AsParallelWriter()
            };

            JobHandle collisionDetectionJobHandle = collisionDetectionJob.Schedule(_rigidbodiesArray.Length, 64);
            collisionDetectionJobHandle.Complete();

            var collisionArray = _collisionIndexesHashSet.ToNativeArray(Allocator.TempJob);

            var collisionResolutionJob = new CollisionResolutionJob()
            {
                rigidbodiesArray = _rigidbodiesArray,
                collisionIndexesArray = collisionArray
            };

            JobHandle collisionResolutionJobHandle = collisionResolutionJob.Schedule();
            collisionResolutionJobHandle.Complete();

            collisionArray.Dispose();

            foreach (var rigidbody in _rigidbodiesArray)
            {
                DeterministicRigidbodyData data = _rigidbodiesDictionary[rigidbody.Index];
                data.SetSimulationData(rigidbody);
                _rigidbodiesDictionary[rigidbody.Index] = data;
            }

            RigidbodyUpdateCompleteEvent?.Invoke(_rigidbodiesDictionary);
        }

        private void CreateCurrentNativeArrays() 
        {
            _rigidbodiesArray = new(_rigidbodiesDictionary.Values.ToArray(), Allocator.Persistent);
            _collisionIndexesHashSet = new(_rigidbodiesDictionary.Count * _rigidbodiesDictionary.Count, Allocator.Persistent);
        }

        private void DisposeCurrentNativeArrays()
        {
            if (_rigidbodiesArray.IsCreated)
                _rigidbodiesArray.Dispose();

            if (_collisionIndexesHashSet.IsCreated)
                _collisionIndexesHashSet.Dispose();
        }

        //[BurstCompile]
        private struct CalculateBoundsJob : IJobParallelFor
        {
            public NativeArray<DeterministicRigidbodyData> rigidbodiesArray;

            public void Execute(int index)
            {
                DeterministicRigidbodyData data = rigidbodiesArray[index];

                switch (data.ColliderType)
                {
                    case ColliderType.None:
                        data.Bounds = new BoundsFp();
                        break;
                    case ColliderType.Box:
                        data.Bounds = GetBoxBounds(data.Position, data.Extents, data.Rotation);
                        break;
                    case ColliderType.Sphere:
                        data.Bounds = GetSphereBounds(data.Position, data.Radius);
                        break;
                    default:
                        break;
                }

                rigidbodiesArray[index] = data;
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
                return new BoundsFp(center, new Vector3Fp(radius, radius, radius));
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

        [BurstCompile]
        private struct DynamicsJob : IJobParallelFor
        {
            public NativeArray<DeterministicRigidbodyData> rigidbodiesArray;
            public Fp deltaTime;

            public void Execute(int index)
            {
                DeterministicRigidbodyData rigidbody = rigidbodiesArray[index];

                rigidbody.Velocity += rigidbody.Acceleration * deltaTime;
                rigidbody.AngularVelocity += rigidbody.AngularAcceleration * deltaTime;

                rigidbodiesArray[index] = rigidbody;
            }
        }

        [BurstCompile]
        private struct CollisionDetectionJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<DeterministicRigidbodyData> rigidbodiesArray;
            public NativeParallelHashSet<int>.ParallelWriter collisionIndexesHashSet;

            public void Execute(int index)
            {
                for (int i = 0; i < rigidbodiesArray.Length; i++)
                {
                    bool differentCollision = i != index;

                    if (!differentCollision)
                        continue;

                    bool aabbIntersects = rigidbodiesArray[i].Bounds.Intersects(rigidbodiesArray[index].Bounds);

                    if (!aabbIntersects)
                        continue;

                    bool layersCollided = SameLayerCollision(rigidbodiesArray[i].CollisionLayer, rigidbodiesArray[index].CollisionLayer);

                    if (!layersCollided)
                        continue;

                    bool collided = true;

                    if (rigidbodiesArray[i].ColliderType == ColliderType.Sphere) 
                    {
                        if (rigidbodiesArray[index].ColliderType == ColliderType.Sphere) 
                        {
                            collided = IsCollidingSphereWithSphere(rigidbodiesArray[i], rigidbodiesArray[index]);
                        }
                        else 
                        {
                            collided = IsCollidingSphereWithBox(rigidbodiesArray[i], rigidbodiesArray[index]);
                        }
                    }
                    else 
                    {
                        if (rigidbodiesArray[index].ColliderType == ColliderType.Sphere)
                        {
                            collided = IsCollidingSphereWithBox(rigidbodiesArray[i], rigidbodiesArray[index]);
                        }
                        else
                        {
                            collided = IsCollidingBoxWithBox(rigidbodiesArray[i], rigidbodiesArray[index]);
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

            private bool IsCollidingSphereWithSphere(DeterministicRigidbodyData sphere1, DeterministicRigidbodyData sphere2) 
            {
                Vector3Fp diff = sphere1.Position - sphere2.Position;
                Fp distanceSquared = MathVector3Fp.SqrtMagnitude(diff);
                Fp radiusSum = sphere1.Radius + sphere2.Radius;
                return distanceSquared <= (radiusSum * radiusSum);
            }

            private bool IsCollidingSphereWithBox(DeterministicRigidbodyData sphere, DeterministicRigidbodyData box)
            {
                return true;
            }

            private bool IsCollidingBoxWithBox(DeterministicRigidbodyData box1, DeterministicRigidbodyData box2)
            {
                return true;
            }

            private int EncodeCollisionDetectionPair(int index1, int index2)
            {
                int smaller = Math.Min(index1, index2);
                int larger = Math.Max(index1, index2);
                return (smaller << 16) | larger;
            }
        }

        [BurstCompile]
        private struct CollisionResolutionJob : IJob
        {
            [ReadOnly] public NativeArray<int> collisionIndexesArray;
            public NativeArray<DeterministicRigidbodyData> rigidbodiesArray;

            public void Execute()
            {
                for (int i = 0; i < collisionIndexesArray.Length; i++)
                {
                    DecodeCollisionDetectionPair(i, out int colliderIndexA, out int colliderIndexB);

                    DeterministicRigidbodyData dataA = rigidbodiesArray[colliderIndexA];

                    DeterministicRigidbodyData dataB = rigidbodiesArray[colliderIndexB];
                }
            }

            private void DecodeCollisionDetectionPair(int encoded, out int index1, out int index2)
            {
                index1 = encoded >> 16;
                index2 = encoded & 0xFFFF;
            }
        }
    }
}
