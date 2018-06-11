namespace SnTraceViewer.Transformations
{
    public abstract class NativeTransformation : Transformation
    {
        public override string Name => GetType().Name;
    }
}
