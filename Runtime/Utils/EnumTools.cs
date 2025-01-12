using System;
using Unity.Collections.LowLevel.Unsafe;

namespace DeterministicPhysicsLibrary.Runtime
{
    //Unashamedly stolen from: https://discussions.unity.com/t/c-hasaflag-method-extension-how-to-not-create-garbage-allocation/729473/13
    public static class EnumTools
    {
        private static int s_UIntSize = UnsafeUtility.SizeOf<uint>();
        private static int s_ULongSize = UnsafeUtility.SizeOf<ulong>();
        private static int s_ByteSize = UnsafeUtility.SizeOf<byte>();

        private static uint NumericType<TEnum>(TEnum t) where TEnum : struct, Enum
        {
            unsafe
            {
                void* ptr = UnsafeUtility.AddressOf(ref t);
                return *((uint*)ptr);
            }
        }
        private static ulong NumericTypeULong<TEnum>(TEnum t) where TEnum : struct, Enum
        {
            unsafe
            {
                void* ptr = UnsafeUtility.AddressOf(ref t);
                return *((ulong*)ptr);
            }
        }

        private static byte NumericTypeByte<TEnum>(TEnum t) where TEnum : struct, Enum
        {
            unsafe
            {
                void* ptr = UnsafeUtility.AddressOf(ref t);
                return *((byte*)ptr);
            }
        }

        public static bool HasFlagUnsafe<TEnum>(TEnum lhs, TEnum rhs) where TEnum : struct, Enum
        {
            int size = UnsafeUtility.SizeOf<TEnum>();
            if (size == s_UIntSize)
            {
                return (NumericType(lhs) & NumericType(rhs)) > 0;
            }
            else if (size == s_ULongSize)
            {
                return (NumericTypeULong(lhs) & NumericTypeULong(rhs)) > 0;
            }
            else if (size == s_ByteSize)
            {
                return (NumericTypeByte(lhs) & NumericTypeByte(rhs)) > 0;
            }
            throw new Exception("No matching conversion function found for an Enum of size: " + size);
        }
    }
}