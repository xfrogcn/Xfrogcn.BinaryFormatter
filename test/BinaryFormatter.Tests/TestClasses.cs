using System;
using System.Collections.Generic;
using System.Text;

namespace Xfrogcn.BinaryFormatter.Tests
{
    class ObjTestA
    {
        public uint A { get; set; }

        public string B { get; set; }

    }

    class ObjTestB : ObjTestA
    {

        public ObjTestB C { get; set; }

        public ObjTestA D { get; set; }

        public int? E { get; set; }
    }

    class TestCtorA
    {
        [BinaryFormatter.Serialization.BinaryConstructor]
        public TestCtorA(string a, uint b)
        {
            A = a;
            B = b;
        }

        public string A { get; set; }

        public uint B { get; set; }

        public string C { get; set; }
    }

    public enum TestEnumA
    {
        None,
        A,
        B
    }


    class TestCtorB : TestCtorA
    {
        public TestCtorB(string a, uint b) : base(a, b)
        {

        }

        public ObjTestA TestA { get; set; }

        public TestEnumA? EnumA { get; set; }
    }

    class TestCtorC : TestCtorB
    {
        [BinaryFormatter.Serialization.BinaryConstructor]
        public TestCtorC(TestCtorB parent, TestCtorA temp) : base(parent.A, parent.B)
        {
            Temp = temp;
            Parent = parent;
        }

        public TestCtorB Parent { get; set; }

        public TestCtorA Temp { get; }
    }
}
