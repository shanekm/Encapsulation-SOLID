using System;
using System.IO;

namespace Ploeh.Samples.Encapsulation.CodeExamples
{
    public class MessageStore // Entry Point
    {
        private readonly IFileLocator fileLocator;
        private readonly IStoreWriter writer;
        private readonly IStoreReader reader;

        public MessageStore(
            IStoreWriter writer,
            IStoreReader reader,
            IFileLocator fileLocator)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");
            if (reader == null)
                throw new ArgumentNullException("reader");
            if (fileLocator == null)
                throw new ArgumentNullException("fileLocator");

            this.fileLocator = fileLocator;
            this.writer = writer;
            this.reader = reader;
        }

        public void Save(int id, string message)
        {
            this.writer.Save(id, message);
        }

        public Maybe<string> Read(int id)
        {
            return this.reader.Read(id); // Decorator => call Logger
        }

        public FileInfo GetFileInfo(int id)
        {
            return this.fileLocator.GetFileInfo(id);
        }
    }
}
