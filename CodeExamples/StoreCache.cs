using System.Collections.Concurrent;
using System.Linq;

namespace Ploeh.Samples.Encapsulation.CodeExamples
{
    public class StoreCache : IStoreWriter, IStoreReader
    {
        private readonly ConcurrentDictionary<int, Maybe<string>> cache;
        private readonly IStoreReader reader;
        private readonly IStoreWriter writer;

        public StoreCache(IStoreWriter writer, IStoreReader reader)
        {
            cache = new ConcurrentDictionary<int, Maybe<string>>();
            this.writer = writer;
            this.reader = reader;
        }

        public Maybe<string> Read(int id)
        {
            Maybe<string> retVal; // Checking here, message doesn't have to be passed in
            if (cache.TryGetValue(id, out retVal))
                return retVal;

            retVal = reader.Read(id); // Decorator => call FileStore
            if (retVal.Any())
                cache.AddOrUpdate(id, retVal, (i, s) => retVal);

            return retVal;
        }

        public void Save(int id, string message)
        {
            writer.Save(id, message); // Decorator => call FileStore
            var m = new Maybe<string>(message);
            cache.AddOrUpdate(id, m, (i, s) => m);
        }
    }
}