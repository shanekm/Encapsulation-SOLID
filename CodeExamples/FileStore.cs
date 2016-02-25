using System;
using System.IO;

namespace Ploeh.Samples.Encapsulation.CodeExamples
{
    /* 
        SOLID - why? - maintance
            - not a framework
            - not a library
            - principles - object oriented design
            - to make your more productive by making code maitainable
            - solid - addresses code smell

        1. Encapsulation - implementation hiding, used for so others can use your code easier    
            2. Why code sucks?
                - extremely difficult to read
                - hard to add features
                - maintainability difficut
                - more time spent reading than writing code
            3. Make API's understandable
            4. CQS
                Commands: return void (hide side effects)
                Query: return objects T or T[]
            5. Postel's Law - giving guarantess when OUT, be tolarent when IN
                Conservative - OUT (strong) specific when send back to client (strong guarantee is good)
                Liberal - IN (broad) - broad when accepting
            6. Input
                - protect invariants / invalid state can be corrupted - when creating new()
                - FailFast - adding guard clause in constructor
            7. Never return null

        REFACTORING 1:
            1. CQS
            2. new() -> anything needs to be set at the time of new(ing)?
            3. can the state of class be modified? get; set; empty?
            4. returning null values
            5. should fields/properties be accessed outside?

             - remove Event for when reading. Not necessary
             - string Save() => returns path => but it's command, should be void, Add new method GetFileName();
             - invariants - protect invariatns, invalid state when creating new() because WorkingDirectory could be set to empty
             - public string WorkingDirectory { get; set; } => new FileStire() needs WorkingDir to be set, so set in constructor
             - public string Read(id) => id unknown, should it return null? Empty.String()?, is null valid sometimes, and other times not valid?

            Returning Null value - should/shouldn't - for purpose of not returning null, null is bad
                1. Tester/Doer -> IfExists(int id) => inside Read() - not thread safe, other thread removed id
                2. TryRead -> bool TryRead(int id, out string msg) 
                      - returns false if not found - not very much OOP (not fluent interface ref/out)
                      - addition complex type to hold msg and error leads to more problems
                3. Maybe -> collection of 0 or 1 elements inside it, Maybe<T> only able to have 0 or 1 elements - see Maybe.cs


        ----------------------------------------------------------------------------------------------------------------------
        2. SRP - how do you define SRP?
            - a class should have only one reason to change (caching, writing, reading)
            - FileStore - caching can change, logging can change, storing - bad! (Logging, Caching, Storage, Orchastration) => may change
             FIX: take out all reasons for change and create new class for it
                a. StoreLogger() => Saving(int id), Saved(int id), Reading(int id) etc
                b. StoreCache() => AddOrUpdate, GetOrAdd etc
            - each class should do one thing and do it well
            - MessageStore - each has it's own reason to exist
                a. StoreLogger
                b. StoreCache
                c. FileStore
            - needless complexity
            - general solutions leads to coupling and complexity => be specific (SRP)

            If Interface/Abstract class can be implemented by ONLY one concrete class then it's to specific
                - this violates reuse principle (to specific)
            FIX:    
                1. Start by creating concrete class behariour
                2. then start looking for common behavior and putting it into interface as commonalities 
                3. Rule of 3:  common cases at least - until you introduce abstraction


        ----------------------------------------------------------------------------------------------------------------------
        3. OCP - open for extendiblity/closed for modification - once used by clients it shouldn't be changed
            - how can you modify behaviour if you can not change/modify original code?
            - composition over inheritance
            - Stranggler pattern - tree taken over by other branches/tree
            - [Obsolete("message", true/false error) - eventually getting rid of old class. Letting know clients by warning or error during build

            FIX:
                1. virtual - abstract classes/inheritance, make methods virtual so that FileLogger can be extended etc

        REFACTORING 2:
            1. FileStore renamed to MessageStore
            2. public class MessageStore(FileStore fs, Cache c, Logger l) : IStoreWriter
            3. created caching, logger etc
            4. DirectoryInfo for WorkingDirectory instead of string


        ----------------------------------------------------------------------------------------------------------------------
        4. LSP - subtypes must be substitutable for their base type
            Violating LSP
            - often violated by attempts to remove features
            - Ex: ReadOnlyCollection<T> : ICollection => throw new NotImplementedException - breaks LSP
            - Ex: Downcasting - when you do a lot of downcasts
            - Ex: Extracted interfaces - vs generates interface for you

        REFACTORING 3:
                - IStore => GetFileInfo pertains to FileStore not other impelmentations (SqlStore : otherwise NotImpelmentedException)
                - FileStore : public virtual FileInfo GetFileInfo(int id, string workingDirectory) can't be implemented by DbSotre 
                    if all these methods are extracted to interface - breaking LSP - change corectness of system

  
        // Starting
        public class FileStore
        {
            public string WorkingDirectory { get; set; } // necessary when new()
            public void Save(int id, string message)
            public string Read(int id)                  // This method may or may not return a string 
                                                        // public string Read(int id) string could be null, empty etc
            public string GetFileName(int id)  
        }




            5. IStore => GetFileInfo pertains to FileStore not other impelmentations (SqlStore : otherwise NotImpelmentedException)
    */

    public class FileStore : IFileLocator, IStoreWriter, IStoreReader
    {
        private readonly DirectoryInfo workingDirectory;

        public FileStore(DirectoryInfo workingDirectory)
        {
            if (workingDirectory == null)
                throw new ArgumentNullException("workingDirectory");
            if (!workingDirectory.Exists)
                throw new ArgumentException("Boo", "workingDirectory");

            this.workingDirectory = workingDirectory;
        }

        //public Write(string path, string message) => id is more abstract, relies on Id
        // GetFileInfo is concrete method in FileStore so it can be called within
        public void Save(int id, string message)
        {
            var path = this.GetFileInfo(id).FullName;
            File.WriteAllText(path, message);
        }

        public Maybe<string> Read(int id)
        {
            var file = this.GetFileInfo(id);        // only used in FileStore, not SqlStore, SqlStore would have to throw NotImplementedException
            if (!file.Exists)                       // if (!file.Exists) => refactored out to FileStore implementation only
                return new Maybe<string>();
            var path = file.FullName;
            return new Maybe<string>(File.ReadAllText(path));
        }

        public FileInfo GetFileInfo(int id)         // GetFileInfo only pertains to FileStore
        {
            return new FileInfo(
                Path.Combine(this.workingDirectory.FullName, id + ".txt"));
        }
    }

}
