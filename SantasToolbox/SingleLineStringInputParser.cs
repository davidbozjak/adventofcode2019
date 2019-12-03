using System.Collections.Generic;
using System.Linq;

namespace SantasToolbox
{
    /// <summary>
    /// A parser, to be used with <see cref="InputProvider"/>, when the input comes as a seperated line of many inputs 
    /// </summary>
    public class SingleLineStringInputParser<T>
    {
        public delegate bool StringToTConverter(string? input, out T result);

        private readonly Queue<T> parserInputs = new Queue<T>();
        private readonly StringToTConverter converter;

        public SingleLineStringInputParser(StringToTConverter converter)
        {
            this.converter = converter;
        }

        public bool GetValue(string? input, out T value)
        {
            value = default(T);

            if (string.IsNullOrWhiteSpace(input))
            {
                if (parserInputs.Count <= 0)
                    return false;
            }
            else
            {
                var itemsToAdd = 
                    input.Split(',')
                    .Where(w => this.converter(w, out _) == true)
                    .Select(w => { T value; this.converter(w, out value); return value; })
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
