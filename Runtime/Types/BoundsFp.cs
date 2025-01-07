using System;
using FixedPoint;
using FixedPoint.SubTypes;

namespace DeterministicPhysicsLibrary.Runtime
{
    public struct BoundsFp : IEquatable<BoundsFp>, IFormattable
    {
        private Vector3Fp _center;
        private Vector3Fp _extents;
        
        public BoundsFp(Vector3Fp center, Vector3Fp size)
        {
            _center = center;
            _extents = size * new Fp(0.5f);
        }
        
        public static bool operator ==(BoundsFp lhs, BoundsFp rhs)
        {
            return lhs._center == rhs._center && lhs._extents == rhs._extents;
        }

        public static bool operator !=(BoundsFp lhs, BoundsFp rhs) => !(lhs == rhs);

        public Vector3Fp Center
        {
            get => _center;
            set => _center = value;
        }

        public Vector3Fp Size
        {
            get => _extents * new Fp(2f);
            set => _extents = value * new Fp(0.5f);
        }

        public Vector3Fp Extents
        {
            get => _extents;
            set => _extents = value;
        }

        public Vector3Fp Min
        {
            get => _center - _extents;
            set => SetMinMax(value, Max);
        }

        public Vector3Fp Max
        {
            get => _center + _extents;
            set => SetMinMax(Min, value);
        }

        public void SetMinMax(Vector3Fp min, Vector3Fp max)
        {
            _extents = (max - min) * new Fp(0.5f);
            _center = min + _extents;
        }
        
        public void Encapsulate(Vector3Fp point)
        {
            SetMinMax(MathVector3Fp.Min(Min, point), MathVector3Fp.Max(Max, point));
        }
            
        public void Encapsulate(BoundsFp bounds)
        {
            Encapsulate(bounds._center - bounds._extents);
            Encapsulate(bounds._center + bounds._extents);
        }
        
        public void Expand(Fp amount)
        {
            amount *= new Fp(0.5f);
            _extents += new Vector3Fp(amount, amount, amount);
        }

        public void Expand(Vector3Fp amount) => _extents += amount * new Fp(0.5f);

        public bool Intersects(BoundsFp bounds)
        {
            return Min.x <= bounds.Max.x && Max.x >= bounds.Min.x &&
                   Min.y <= bounds.Max.y && Max.y >= bounds.Min.y &&
                   Min.z <= bounds.Max.z && Max.z >= bounds.Min.z;
        }
        
        public bool Contains(Vector3Fp point)
        {
            return point.x <= Max.x && point.x >= Min.x &&
                   point.y <= Max.y && point.y >= Min.y &&
                   point.z <= Max.z && point.z >= Min.z;
        }
        
        public Vector3Fp ClosestPoint(Vector3Fp point)
        {
            Vector3Fp minPoint = Min;
            Vector3Fp maxPoint = Max;
            Vector3Fp returnPoint = new Vector3Fp
            {
                x = MathFp.Clamp(point.x, minPoint.x, maxPoint.x),
                y = MathFp.Clamp(point.y, minPoint.y, maxPoint.y),
                z = MathFp.Clamp(point.z, minPoint.z, maxPoint.z)
            };

            return returnPoint;
        }

        public Fp SqrDistance(Vector3Fp point)
        {
            Vector3Fp boundsPoint = ClosestPoint(point);
            Vector3Fp dirVector = boundsPoint - point;
            return MathVector3Fp.Dot(dirVector, dirVector);
        }

        public override int GetHashCode()
        {
            Vector3Fp fixedVector3 = _center;
            int hashCode = fixedVector3.GetHashCode();
            fixedVector3 = _extents;
            int num = fixedVector3.GetHashCode() << 2;
            return hashCode ^ num;
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return $"Center: {_center.ToString("0.##########", formatProvider)}," +
                   $"Extents: {_extents.ToString("0.##########", formatProvider)}";
        }

        public override bool Equals(object other) => other is BoundsFp other1 && this.Equals(other1);

        public bool Equals(BoundsFp other)
        {
            return _center.Equals(other._center) && _extents.Equals(other._extents);
        }
    }
}
