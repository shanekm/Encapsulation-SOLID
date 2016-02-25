namespace Ploeh.Samples.Encapsulation.CodeExamples
{
    public interface IStoreWriter
    {
        void Save(int id, string message);
    }
}