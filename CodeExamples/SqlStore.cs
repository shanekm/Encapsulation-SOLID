namespace Ploeh.Samples.Encapsulation.CodeExamples
{
    public class SqlStore : IStoreReader, IStoreWriter
    {
        public Maybe<string> Read(int id)
        {
            // Read from database here
            return new Maybe<string>();
        }

        public void Save(int id, string message)
        {
            // Write to database here
        }
    }
}