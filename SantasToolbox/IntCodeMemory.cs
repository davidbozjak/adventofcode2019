using System.Collections.Generic;
using System.Linq;

namespace SantasToolbox
{
    public interface IIntReadOnlyCodeMemory
    {
        IIntReadOnlyCodeMemory Clone();

        IIntCodeMemory CloneWriteable();

        int this[int index] { get; }

        int Length { get; }
    }

    public interface IIntCodeMemory : IIntReadOnlyCodeMemory
    {
        new int this[int index] { get; set; }
    }

    public class IntCodeMemory : IIntCodeMemory
    {
        private readonly int[] memory;

        public IntCodeMemory(IEnumerable<int> inputProvider)
        {
            this.memory = inputProvider.ToArray();
        }

        public int this[int index] 
        { 
            get => this.memory[index];
            set => this.memory[index] = value; 
        }

        public int Length => this.memory.Length;

        public IIntReadOnlyCodeMemory Clone() => new IntCodeMemory(this.memory);

        public IIntCodeMemory CloneWriteable() => new IntCodeMemory(this.memory);

    }
}
