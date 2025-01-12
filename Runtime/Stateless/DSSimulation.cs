using System;
using System.Linq;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Collections;

namespace DeterministicPhysicsLibrary.Runtime.Stateless
{
    public class DSSimulation
    {
        public event Action<Dictionary<int, DSRigidbodyData>> RigidbodyUpdateCompleteEvent;

        private NativeArray<DSRigidbodyData> _dataArray;
        private NativeParallelHashSet<int> _collisionIndexesHashSet;

        private Dictionary<int, DSRigidbodyData> _dataDictionary;
        
        private Queue<int> _freeIndexesQueue;
        private int _rigidbodyIndex;

        private const int collisionPasses = 6;

        public DSSimulation() 
        {
            _dataDictionary = new Dictionary<int, DSRigidbodyData>();
            _freeIndexesQueue = new Queue<int>();
        }

        ~DSSimulation() 
        {
            DisposeNativeDataArrays();
        }

        public int AddRigidbody(DSRigidbodyInputData rigidbodyInputData) 
        {
            int index = GetFreeIndex();

            DSRigidbodyData data = new() 
            {
                index = index,
                input = rigidbodyInputData
            };
            _dataDictionary.Add(index, data);

            DisposeNativeDataArrays();
            CreateNativeDataArrays();
            
            return index;
        }

        public void UpdateRigidbodyData(int rigidbodyIndex, DSRigidbodyInputData rigidbodyInputData)
        {
            DSRigidbodyData data = _dataDictionary[rigidbodyIndex];
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

        public void UpdateSimulation(float deltaTime) 
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

            for (int i = 0; i < collisionPasses; i++)
            {
                _collisionIndexesHashSet.Clear();

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

                collisionArray.Dispose();

                foreach (var data in _dataArray)
                {
                    DSRigidbodyData newData = _dataDictionary[data.index];
                    newData.output = data.output;
                    _dataDictionary[data.index] = newData;
                }
                RigidbodyUpdateCompleteEvent?.Invoke(_dataDictionary);
            }
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