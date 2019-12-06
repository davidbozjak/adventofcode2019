using System;
using System.Collections.Generic;
using System.Linq;

namespace SantasToolbox
{
    public class UniqueFactory<T, U>
            where U : IComparable
    {
        private readonly List<T> allCreatedInstances = new List<T>();
        private readonly Func<T, U> identifierFunc;
        private readonly Func<U, T> constructingFunc;

        public UniqueFactory(Func<T, U> identifierFunc, Func<U, T> constructingFunc)
        {
            this.identifierFunc = identifierFunc;
            this.constructingFunc = constructingFunc;
        }

        public IReadOnlyList<T> AllCreatedInstances => this.allCreatedInstances.AsReadOnly();

        public T GetOrCreateInstance(U identifier)
        {
            var instance = this.allCreatedInstances.FirstOrDefault(w => this.identifierFunc(w).CompareTo(identifier) == 0);

            if (instance != null) return instance;

            instance = this.constructingFunc(identifier);

            allCreatedInstances.Add(instance);

            return instance;
        }
    }
}
