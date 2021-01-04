using System;

namespace Xfrogcn.BinaryFormatter.Tests
{
    public enum DiffTestEnumA
    {
        None,
        A,
        B
    }

    interface DiffIEmptyInterface  { }


    struct DiffStructA : DiffIEmptyInterface
    {
        public int? A { get; set; }

        public int B;

        public String C { get; set; }

        public TestEnumA EA { get; set; }

        public TestEnumA? EB { get; set; }

    }

 
    class DiffObjTestA
    {
        public uint A { get; set; }

        public string B { get; set; }

    }

    class DiffObjTestB : DiffObjTestA
    {

        public DiffObjTestB C { get; set; }

        public DiffObjTestA D { get; set; }

        public int? E { get; set; }
    }

    class DiffTestCtorA : IComparable
    {
        [BinaryFormatter.Serialization.BinaryConstructor]
        public DiffTestCtorA(string a, uint b)
        {
            A = a;
            B = b;
        }

        public string A { get; set; }

        public uint B { get; set; }

        public string C { get; set; }

        public int CompareTo(object obj)
        {
            if(obj == null)
            {
                return 0;
            }
            return (int)(this.B - (obj as DiffTestCtorA).B);
        }
    }




    class DiffTestCtorB : DiffTestCtorA
    {
        public DiffTestCtorB(string a, uint b) : base(a, b)
        {

        }

        public DiffObjTestA TestA { get; set; }

        public DiffTestEnumA? EnumA { get; set; }
    }

    class DiffTestCtorC : DiffTestCtorB
    {
        [BinaryFormatter.Serialization.BinaryConstructor]
        public DiffTestCtorC(DiffTestCtorB parent, DiffTestCtorA temp) : base(parent.A, parent.B)
        {
            Temp = temp;
            Parent = parent;
        }

        public DiffTestCtorB Parent { get; set; }

        public DiffTestCtorA Temp { get; }
    }

   
    class DiffTestSelfRefA
    {
        public string A { get; set; }
        public DiffTestSelfRefA Self { get; set; }
    }

    class DiffTestRef<T>
    {
        public T A { get; set; }

        public T B { get; set; }
    }

    class DiffSelfRefWithCtorA
    {
        [BinaryFormatter.Serialization.BinaryConstructor]
        public DiffSelfRefWithCtorA(string a)
        {
            A = a;
        }

        public string A { get; set; }

        public DiffSelfRefWithCtorA Self { get; set; }
    }

    class DiffTestRefWithCtor
    {
        [BinaryFormatter.Serialization.BinaryConstructor]
        public DiffTestRefWithCtor(DiffTestRefWithCtor parent)
        {
            Parent = parent;
            if(parent != null)
            {
                parent.Child = this;
            }
            
        }

        public string A { get; set; }

        public DiffTestRefWithCtor Parent { get; }

        public DiffTestRefWithCtor Child { get; set; }
    }
}
