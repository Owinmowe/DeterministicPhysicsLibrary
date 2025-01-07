using DeterministicPhysicsLibrary.Runtime;
using FixedPoint.SubTypes;
using NUnit.Framework;
using UnityEngine;

namespace DeterministicPhysicsLibrary.Tests
{
    public class BoundsFpTests
    {
        [Test]
        public void CreationTest()
        {
            Vector3 testBoundSize = new Vector3(12.2f, -.213f, -5f);
            Vector3 testBoundCenter = new Vector3(-.2f, -12312f, 1f);
            Bounds testBounds = new Bounds(center: testBoundCenter, size: testBoundSize);
            
            BoundsFp testBoundsFixed = new BoundsFp((Vector3Fp)testBounds.center, (Vector3Fp)testBounds.size);

            float result = (testBounds.center - (Vector3)testBoundsFixed.Center).magnitude +
                           (testBounds.size - (Vector3)testBoundsFixed.Size).magnitude;
            
            Assert.True(result < Mathf.Epsilon);
        }

        [Test]
        public void SetMinMaxTest()
        {
            Vector3 testBoundSize = new Vector3(12.2f, -.213f, -5f);
            Vector3 testBoundCenter = new Vector3(-.2f, -12312f, 1f);
            Bounds testBounds = new Bounds(center: testBoundCenter, size: testBoundSize);
            Vector3 testBoundMin = new Vector3(4f, -3.2f, -1.54f);
            Vector3 testBoundMax = new Vector3(4.2f, -1.5f, -1f);
            testBounds.SetMinMax(testBoundMin, testBoundMax);
            
            BoundsFp testBoundsFixed = new BoundsFp((Vector3Fp)testBounds.center, (Vector3Fp)testBounds.size);
            testBoundsFixed.SetMinMax((Vector3Fp)testBoundMin, (Vector3Fp)testBoundMax);
            
            float result = (testBounds.center - (Vector3)testBoundsFixed.Center).magnitude +
                           (testBounds.size - (Vector3)testBoundsFixed.Size).magnitude;
            
            Assert.True(result < Mathf.Epsilon);
        }

        [Test]
        public void EncapsulateTest()
        {
            Vector3 testBoundSize = new Vector3(12.2f, -.213f, -5f);
            Vector3 testBoundCenter = new Vector3(-.2f, -12312f, 1f);
            Bounds testBounds = new Bounds(center: testBoundCenter, size: testBoundSize);
            
            BoundsFp testBoundsFixed = new BoundsFp((Vector3Fp)testBounds.center, (Vector3Fp)testBounds.size);

            Vector3 testPoint = new Vector3(20.2f, -69.213f, 10f);
            testBounds.Encapsulate(testPoint);
            testBoundsFixed.Encapsulate(new Vector3Fp(testPoint));
            
            float result = (testBounds.center - (Vector3)testBoundsFixed.Center).magnitude +
                           (testBounds.size - (Vector3)testBoundsFixed.Size).magnitude;
            
            Assert.True(result < Mathf.Epsilon);
        }

        [Test]
        public void ExpandTest()
        {
            Vector3 testBoundSize = new Vector3(12.2f, -.213f, -5f);
            Vector3 testBoundCenter = new Vector3(-.2f, -12312f, 1f);
            Bounds testBounds = new Bounds(center: testBoundCenter, size: testBoundSize);
            
            BoundsFp testBoundsFixed = new BoundsFp((Vector3Fp)testBounds.center, (Vector3Fp)testBounds.size);

            Vector3 testAmount = new Vector3(20.2f, -69.213f, 10f);
            testBounds.Expand(testAmount);
            testBoundsFixed.Expand(new Vector3Fp(testAmount));
            
            float result = (testBounds.center - (Vector3)testBoundsFixed.Center).magnitude +
                           (testBounds.size - (Vector3)testBoundsFixed.Size).magnitude;
            
            Assert.True(result < Mathf.Epsilon);
        }
        
        [Test]
        public void IntersectsTest()
        {
            Vector3 testBoundCenter1 = new Vector3(0, 0, 0);
            Vector3 testBoundSize1 = new Vector3(2, 2, 2);
            Bounds testBounds1 = new Bounds(center: testBoundCenter1, size: testBoundSize1);
            
            Vector3 testBoundCenter2 = new Vector3(0, 1, 1);
            Vector3 testBoundSize2 = new Vector3(2, 2, 2);
            Bounds testBounds2 = new Bounds(center: testBoundCenter2, size: testBoundSize2);
            
            BoundsFp testBoundsFixed1 = new BoundsFp((Vector3Fp)testBounds1.center, (Vector3Fp)testBounds1.size);
            BoundsFp testBoundsFixed2 = new BoundsFp((Vector3Fp)testBounds2.center, (Vector3Fp)testBounds2.size);
            
            Assert.True(testBoundsFixed1.Intersects(testBoundsFixed2) == testBounds1.Intersects(testBounds2));
        }
        
        [Test]
        public void ContainsTest()
        {
            Vector3 testBoundCenter = new Vector3(-.2f, -12312f, 1f);
            Vector3 testBoundSize = new Vector3(12.2f, -.213f, -5f);
            Bounds testBounds = new Bounds(center: testBoundCenter, size: testBoundSize);
            
            BoundsFp testBoundsFixed = new BoundsFp((Vector3Fp)testBounds.center, (Vector3Fp)testBounds.size);

            Vector3 testPoint = new Vector3(10.2f, -12312.1f, 3f);
            
            Assert.True(testBounds.Contains(testPoint) == testBoundsFixed.Contains(new Vector3Fp(testPoint)));
        }
        
        [Test]
        public void ClosestPointTest()
        {
            Vector3 testBoundSize = new Vector3(12.2f, -.213f, -5f);
            Vector3 testBoundCenter = new Vector3(-.2f, -12312f, 1f);
            Bounds testBounds = new Bounds(center: testBoundCenter, size: testBoundSize);
            
            BoundsFp testBoundsFixed = new BoundsFp((Vector3Fp)testBounds.center, (Vector3Fp)testBounds.size);

            Vector3 testPoint = new Vector3(20.2f, -69.213f, 10f);
            Vector3 closestPointFloat = testBounds.ClosestPoint(testPoint);
            Vector3 closestPointFixed = (Vector3)testBoundsFixed.ClosestPoint(new Vector3Fp(testPoint));
            
            float result = (closestPointFloat - closestPointFixed).magnitude;
            
            Assert.True(result < Mathf.Epsilon);
        }
        
        [Test]
        public void SqrDistanceTest()
        {
            Vector3 testBoundSize = new Vector3(12.2f, -.213f, -5f);
            Vector3 testBoundCenter = new Vector3(-.2f, -12312f, 1f);
            Bounds testBounds = new Bounds(center: testBoundCenter, size: testBoundSize);
            
            BoundsFp testBoundsFixed = new BoundsFp((Vector3Fp)testBounds.center, (Vector3Fp)testBounds.size);

            Vector3 testPoint = new Vector3(20.2f, -69.213f, 10f);
            float sqrDistanceFloat = testBounds.SqrDistance(testPoint);
            float sqrDistanceFixed = (float)testBoundsFixed.SqrDistance(new Vector3Fp(testPoint));
            
            float result = Mathf.Abs(sqrDistanceFloat - sqrDistanceFixed);
            
            Assert.True(result < Mathf.Epsilon);
        }
    }
}
