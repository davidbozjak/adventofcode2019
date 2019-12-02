using System.Collections.Generic;
using System.Linq;

namespace SantasToolbox
{
    /// <summary>
    /// A parser, to be used with <see cref="InputProvider"/>, when the input comes as a seperated line of many inputs 
    /// </summary>
    public class SingleLineStringInputParser
    {
        private readonly Queue<int> parserInputs = new Queue<int>();

        public bool GetInt(string? input, out int value)
        {
            value = -1;

            if (string.IsNullOrWhiteSpace(input))
            {
                if (parserInputs.Count <= 0)
                    return false;
            }
            else
            {
                var itemsToAdd = 
                    input.Split(',')
                    .Where(w => int.TryParse(w, out _) == true)
                    .Select(w => int.Parse(w))
                    .ToList();

                itemsToAdd.ForEach(w => this.parserInputs.Enqueue(w));
            }

            if (this.parserInputs.Count > 0)
            {
                value = this.parserInputs.Dequeue();
                return true;
            }
            else return false;
        }
    }
}
