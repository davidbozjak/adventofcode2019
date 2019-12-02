using System.Collections.Generic;
using System.Linq;

namespace _2_Intcode
{
    interface IIntCodeMemory
    {
        IIntCodeMemory Clone();

        int this[int index] { get; set; }

        int Length { get; }
    }

    class IntCodeMemory : IIntCodeMemory
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

        public IIntCodeMemory Clone() => new IntCodeMemory(this.memory);

    }
}
