using System;
using System.Runtime.InteropServices;
using CS.TaskScheduling;
using NUnit.Framework;

namespace CS.SwarmTask.Tests.TempTest
{
    [TestFixture]
    public class TempTests
    {
        [Test]
        public void Int32Test()
        {
            Console.WriteLine($"int.MinValue:{int.MinValue};int.MaxValue:{int.MaxValue}");
            Console.WriteLine($"2^32-1:{(2 ^ 32 - 1)}");
            Console.WriteLine($"2^(32-1):{(2 ^ (32 - 1))}");

            Console.WriteLine($"{Math.Pow(2, 32 - 1) - 1}");

            Assert.IsEmpty("");
        }

        [Test]
        public void FlagTest()
        {
           
        }

        [Test]
        public void MemeryAlingTest()
        {
            Console.WriteLine(Marshal.SizeOf(typeof (StructDeft)));
            Console.WriteLine(Marshal.SizeOf(typeof (BadStruct)));
            Console.WriteLine(Marshal.SizeOf(typeof (AutoStruct)));

            Assert.IsEmpty("");
        }

        //[StructLayout(LayoutKind.Sequential, Pack = 1)] //这个就是16占用 了
        private struct StructDeft //C#编译器会自动在上面运用[StructLayout(LayoutKind.Sequential)]  //会自动按最大的类型的字节数来对齐,所以是24
        {
            private bool i; //1Byte
            private double c; //8byte
            private bool b; //1byte
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct BadStruct
        {
            [FieldOffset(0)] public bool i; //1Byte
            [FieldOffset(0)] public double c; //8byte
            [FieldOffset(0)] public bool b; //1byte
        }

        [StructLayout(LayoutKind.Auto)]
        private struct AutoStruct
        {
            public bool i; //1Byte
            public double c; //8byte
            public bool b; //1byte
        }
    }
}