﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Ploeh.Samples.Encapsulation.CodeExamples
{
    /* 
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
     
    Refactoring:   
        1. CQS
        2. new() -> can the state of class be modified? get; set; empty?
        3. returning null values

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


    */

    public class FileStore
    {
        public FileStore(string workingDirectory)
        {
            if (workingDirectory == null)
                throw new ArgumentNullException("workingDirectory");
            if (!Directory.Exists(workingDirectory))
                throw new ArgumentException("Boo", "workingDirectory");

            this.WorkingDirectory = workingDirectory;
        }

        public string WorkingDirectory { get; private set; } // this used to be get;set; necessary when new()

        public void Save(int id, string message) 
        {
            var path = this.GetFileName(id);
            File.WriteAllText(path, message);
        }

        // This method may or may not return a string
        public Maybe<string> Read(int id) // public string Read(int id) string could be null, empty etc
        {
            var path = this.GetFileName(id);
            if (!File.Exists(path))
                return new Maybe<string>();
            var message = File.ReadAllText(path);
            return new Maybe<string>(message);
        }

        public string GetFileName(int id)
        {
            return Path.Combine(this.WorkingDirectory, id + ".txt");
        }
    }
}
