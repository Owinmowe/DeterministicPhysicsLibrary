using System;
using System.Linq;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Collections;
using FixedPoint;
using FixedPoint.SubTypes;

namespace DeterministicPhysicsLibrary.Runtime.Managed
{
    public class DMSimulation
    {
        public event Action<Dictionary<int, DMRigidbodyData>> RigidbodyUpdateCompleteEvent;

        private NativeArray<DMRigidbodyData> _dataArray;
        private NativeParallelHashSet<int> _collisionIndexesHashSet;

        private Dictionary<int, DMRigidbodyData> _dataDictionary;

        private Queue<int> _freeIndexesQueue;
        private int _rigidbodyIndex;

        public DMSimulation()
        {
            _dataDictionary = new Dictionary<int, DMRigidbodyData>();
            _freeIndexesQueue = new Queue<int>();
        }

        ~DMSimulation()
        {
            DisposeNativeDataArrays();
        }

        public int AddRigidbody(DMRigidbodyInputData rigidbodyInputData, Vector3Fp startPosition, QuaternionFp startRotation)
        {
            int index = GetFreeIndex();

            DMRigidbodyData data = new()
            {
                index = index,
                input = rigidbodyInputData
            };
            data.simData.Position = startPosition;
            data.simData.Rotation = startRotation;

            _dataDictionary.Add(index, data);

            DisposeNativeDataArrays();
            CreateNativeDataArrays();

            return index;
        }

        public void UpdateRigidbodyData(int rigidbodyIndex, DMRigidbodyInputData rigidbodyInputData)
        {
            DMRigidbodyData data = _dataDictionary[rigidbodyIndex];
            data.input = rigidbodyInputData;

            _dataArray[rigidbodyIndex] = data;
            _dataDictionary[rigidbodyIndex] = data;
        }

        public void RemoveRigidbody(int rigidbodyIndex)
        {
            _dataDictionary.Remove(rigidbodyIndex);
            _freeIndexesQueue.Enqueue(rigidbodyIndex);

            DisposeNativeDataArrays();
            CreateNativeDataArrays();
        }

        public void UpdateSimulation(Fp deltaTime)
        {
            if (!_dataArray.IsCreated || !_collisionIndexesHashSet.IsCreated)
                return;

            var dynamicsJob = new DynamicsJob()
            {
                rigidbodiesData = _dataArray,
                deltaTime = deltaTime
            };

            JobHandle dynamicsJobHandle = dynamicsJob.Schedule(_dataArray.Length, 64);
            dynamicsJobHandle.Complete();

            var calculateBoundsJob = new CalculateBoundsJob()
            {
                rigidbodiesData = _dataArray
            };
            JobHandle calculateBoundsJobHandle = calculateBoundsJob.Schedule(_dataArray.Length, 64);
            calculateBoundsJobHandle.Complete();

            var collisionDetectionJob = new CollisionDetectionJob()
            {
                rigidbodiesData = _dataArray,
                collisionIndexesHashSet = _collisionIndexesHashSet.AsParallelWriter()
            };

            JobHandle collisionDetectionJobHandle = collisionDetectionJob.Schedule(_dataArray.Length, 64);
            collisionDetectionJobHandle.Complete();

            var collisionArray = _collisionIndexesHashSet.ToNativeArray(Allocator.TempJob);

            var collisionResolutionJob = new CollisionResolutionJob()
            {
                rigidbodiesData = _dataArray,
                collisionIndexesArray = collisionArray
            };
            JobHandle collisionResolutionJobHandle = collisionResolutionJob.Schedule();
            collisionResolutionJobHandle.Complete();

            _collisionIndexesHashSet.Clear();
            collisionArray.Dispose();

            foreach (var data in _dataArray)
            {
                DMRigidbodyData newData = _dataDictionary[data.index];
                newData.simData = data.simData;
                newData.output = data.output;
                _dataDictionary[data.index] = newData;
            }

            RigidbodyUpdateCompleteEvent?.Invoke(_dataDictionary);
        }

        private int GetFreeIndex()
        {
            int newIndex = 0;

            if (_freeIndexesQueue.Count > 0)
            {
                newIndex = _freeIndexesQueue.Dequeue();
            }
            else
            {
                newIndex = _rigidbodyIndex;
                _rigidbodyIndex++;
            }

            return newIndex;
        }

        private void CreateNativeDataArrays()
        {
            _dataArray = new(_dataDictionary.Values.ToArray(), Allocator.Persistent);
            _collisionIndexesHashSet = new(_dataDictionary.Count * _dataDictionary.Count, Allocator.Persistent);
        }

        private void DisposeNativeDataArrays()
        {
            if (_dataArray.IsCreated)
                _dataArray.Dispose();

            if (_collisionIndexesHashSet.IsCreated)
                _collisionIndexesHashSet.Dispose();
        }
    }
}