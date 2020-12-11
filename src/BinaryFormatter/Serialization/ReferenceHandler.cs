namespace Xfrogcn.BinaryFormatter.Serialization
{
    public abstract class ReferenceHandler
    {
        public abstract ReferenceResolver CreateResolver(bool writing);
    }
}
