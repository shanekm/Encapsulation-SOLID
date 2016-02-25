using System.Linq;
using Serilog;

namespace Ploeh.Samples.Encapsulation.CodeExamples
{
    public class StoreLogger : IStoreWriter, IStoreReader
    {
        private readonly ILogger log;
        private readonly IStoreReader reader;
        private readonly IStoreWriter writer;

        public StoreLogger(ILogger log, IStoreWriter writer, IStoreReader reader)
        {
            this.log = log;
            this.writer = writer;
            this.reader = reader;
        }

        public Maybe<string> Read(int id)
        {
            log.Debug("Reading message {id}.", id);
            var retVal = reader.Read(id);
            if (retVal.Any())
                log.Debug("Returning message {id}.", id);
            else
                log.Debug("No message {id} found.", id);
            return retVal;
        }

        public void Save(int id, string message)
        {
            log.Information("Saving message {id}.", id);
            writer.Save(id, message);
            log.Information("Saved message {id}.", id);
        }
    }
}