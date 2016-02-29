using System;
using System.IO;

namespace Ploeh.Samples.Encapsulation.CodeExamples
{
    /* 
        QUESTIONS TO ASK WHEN REFACTORING:
            1. new() -> anything needs to be set at the time of new(ing)? How to I prevent this class to be in invalid state?
            2. Protect invariants (guard clauses) - FailFast
            3. CQS -> return void : Commands / return T Queries
            4. can the state of class be modified? get; set; empty?
            5. returning null values: (Tester/Doer, TryRead, Maybe<T>)
            6. should fields/properties be accessed outside?
            7. Pastel's Law -> IN broad / OUT specific (stronger guarantees are good)
            8. Is this a details of the component used/injected or implementation of this class? 
            9. Does class have only one reason to change? or other components may change too?

        SOLID - why? - maintance
            - not a framework
            - not a library
            - principles - object oriented design
            - to make your more productive by making code maitainable
            - solid - addresses code smell
            - abstractions are discovered - by looking for comminalities

        1. Encapsulation - implementation hiding
            WHY?: How do I get it so it won't get in invalid state so that others can use your code easier 
                without throwing errors because set up wrong etc and should not be aware of implementation details

            2. Why code sucks?
                - extremely difficult to read
                - hard to add features
                - maintainability difficut
                - more time spent reading than writing code
            3. Make API's understandable
            4. CQS - encapsulation technique
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
        2. SRP (Single responsibility principle) - be specific / reuse - start extracting granual common behaviour
            WHY?: because general/large solutions leads to coupling and complexity => so be specific (SRP) - lots of small classes
                A class should have only one reason to change (caching, writing, reading)
            
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

            If Interface/Abstract class can be implemented by ONLY one concrete class then it's to specific
                - this violates reuse principle (to specific)
            FIX:    
                1. Start by creating concrete class behariour
                2. then start looking for common behavior and putting it into interface as commonalities 
                3. Rule of 3:  common cases at least - until you introduce abstraction


        ----------------------------------------------------------------------------------------------------------------------
        3. OCP (Open closed principle) - open for extendiblity/closed for modification
            WHY?: because once used by clients it shouldn't be changed, if changed - clients will break/breaking change

            - how can you modify behaviour if you can not change/modify original code?
            - composition over inheritance
            - Stranggler pattern - tree taken over by other branches/tree
            - [Obsolete("message", true/false error) - eventually getting rid of old class. Letting know clients by warning or error during build

            FIX:
                1. virtual - abstract classes/inheritance, make methods virtual so that FileLogger can be extended etc
                2. Strategy / Decorator / Composition patterns to allow for OCP
                3. PricingCalculator => list of rules (Strategy) injected into Calculator

        REFACTORING 2:
            - FileStore renamed to MessageStore
            - public class MessageStore(FileStore fs, Cache c, Logger l) : IStoreWriter
            - created caching, logger etc
            - DirectoryInfo for WorkingDirectory instead of string


        ----------------------------------------------------------------------------------------------------------------------
        4. LSP (Liskov subsitution principle) - subtypes must be substitutable for their base type
            WHY?: because Clients should consume any implementation without changing correctness of the system
                and should not be aware of implementation details

            IMPORTANT: Is it implementation detail? (GetFileInfo pertains to FileStore only)

            Violating LSP
            - often violated by attempts to remove features
            - Ex: ReadOnlyCollection<T> : ICollection => throw new NotImplementedException - breaks LSP
            - Ex: Downcasting - when you do a lot of downcasts
            - Ex: Extracted interfaces - vs generates interface for you
            - Ex: if/else smells => if Manager PringManager else PrintEmployee where Manager : Employee
            
            Fix:
                - Each class has it's own implementation of method for calculating Area or specific calculation
                - Tell, don't ask principle : calculateArea(Rectangle, Square) => Rectangle : Square - each subclass has it's own Cacl() details
                abstract Shape { abstract int Area() } => Area() method implemented by EACH subclass
                Shape inherited by Square and Rectanble with property SideLength

                1. each subclass implements abstract Calc() - it's own implementation detail
                2. if it only relates to one given subclass create new interface and implement by only that class
                3. or create an abstract method that each subclass overwrites (tells) how it does something

        REFACTORING 3:
                - GetFileInfo pertains to FileStore not other impelmentations (SqlStore : otherwise NotImpelmentedException)
                - FileStore : public virtual FileInfo GetFileInfo(int id, string workingDirectory) can't be implemented by DbSotre 
                    if all these methods are extracted to interface - breaking LSP - change corectness of system


        ----------------------------------------------------------------------------------------------------------------------
        5. ISP (Interface segregation principle) - granularity (dragon) - Interface segragation - fined grained classes
            WHY?: Smaller peaces are better, can be reused. Easy to add features, but how do you SUBTRACT features?
                with ISP, if client doesn't need that feature then extract it to new interface

            - helps solve LSP/SRP getting rid of NotImplementedException
            - lots of classes is good thing
            - provides loose coupling => IMPORTANT - client defines what it needs
            - ROLE interfaces over header interfaces - Interfaces should be designed for Roles - define it's memebers (one role => extreme => good)

            FIX:
                1. functional interfaces / Role interfaces
                2. create new interface for unit testing only => ISettingsConfiguration and ISettingsConfiguration2 for unit testing
                3. IConfiguration : IApplication (inheritance of interfaces) - Extending interfaces (old stuff still works) 

        REFACTORING 4:
                - GetFileInfo - shouldn't rely on IStore - only client FileStore requires GetFileInfo, so ISP is useful
                - this.FileLocator : IFileLocator => GetFileInfo(id), no other clients depend on GetFileInfo of IStore so it's removed
                - Role Interfaces - rewriting


        ----------------------------------------------------------------------------------------------------------------------
        6. DIP (Dependency inversion principle) - dependency inversion principle
            WHY?: Clients own abstraction and should not care about implementation details (pluggable architecture)

            - high level modules should not depend on low level modules
            - abstraction should not depend opon details / details should not depend on abstraction
            - composition - better than inheritance because allows for multiple inheritance (implement multiple interfaces)
                    a. composite pattern
                    b. decorator pattern

            Violating DIP:
                - new() instances within a class - explicit
                - static class/method calls
                - System.Date - predicate system classes

            FIX:
                1. IoC
                2. Decorator/composite pattern

        REFACTORING 5:
                - Logger implements Writer/Reader
                - Cache implements Writer/Reader
                - Using decorator and composite pattern one calls another and build from the top (see unit tests)


        ----------------------------------------------------------------------------------------------------------------------
        // Starting
        public class FileStore
        {
            public string WorkingDirectory { get; set; } // necessary when new()
            public void Save(int id, string message)
            public string Read(int id)                  // This method may or may not return a string 
                                                        // public string Read(int id) string could be null, empty etc
            public string GetFileName(int id)  
        }

            
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

        // public Write(string path, string message) => id is more abstract, relies on Id
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


    // Composition
    // - work well with CQS - commands
    // Better than inheritance because it allows multiple inheritance
    // 2 ways to achieve composition
    //  - Composite pattern
    //  - Decorator pattern
    // MessageStore can now create new CompositeStoreWriter() => one line Save() calling all Save() IStoreWriters implementations
    public class CompositeStoreWriter : IStoreWriter
    {
        private readonly IStoreWriter[] writers;

        public CompositeStoreWriter(params IStoreWriter[] writers)
        {
            this.writers = writers;
        }

        public void Save(int id, string message)
        {
            foreach (var storeWriter in this.writers)
            {
                storeWriter.Save(id, message);
            }
        }
    }

}
