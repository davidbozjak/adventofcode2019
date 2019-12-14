using SantasToolbox;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace _14_TBN
{
    class Program
    {
        static void Main(string[] args)
        {
            using var inputProvider = new InputProvider<string>("Input.txt", (string? input, out string value) => { var isnull = string.IsNullOrWhiteSpace(input); value = input ?? string.Empty; return !isnull; });

            Part1(inputProvider);

            //Console.WriteLine("Press any key to continue to Part2");
            //Console.ReadKey();

            //inputProvider.Reset();

            //Part2(inputProvider);
        }

        private static void Part1(IEnumerable<string> input)
        {
            var reactionCookBook = new UniqueFactory<string, Reaction>(w => new Reaction(w));

            foreach (var reaction in input)
            {
                reactionCookBook.GetOrCreateInstance(reaction);
            }

            foreach (var reaction in reactionCookBook.AllCreatedInstances)
            {
                reaction.Complexity = GetComplexity(reaction, 0);
            }

            var fullIngredientsList = new List<Ingredient>();

            var fuelReaction = reactionCookBook.AllCreatedInstances.First(w => w.ResultChemical == "FUEL");

            FillIngredientsList(fuelReaction, 1, fullIngredientsList, reactionCookBook.AllCreatedInstances);

            reactionCookBook.AllCreatedInstances.Where(w => w.ResultChemical == "ORE").ToList().ForEach(w => w.Complexity = 1);

            Console.WriteLine();
            Console.WriteLine("Done Part1");
            foreach (Ingredient ingredient in fullIngredientsList)
            {
                Console.WriteLine($"{ingredient.Chemical} {ingredient.Amount}");
            }

            int GetComplexity(Reaction reaction, int c)
            {
                var nonOreInputs = reaction
                    .InputChemicals
                    .Where(w => w.Chemical != "ORE");

                if (nonOreInputs.Any())
                {
                    return nonOreInputs
                        .Select(w => GetComplexity(reactionCookBook.AllCreatedInstances.First(reaction => reaction.ResultChemical == w.Chemical), c))
                        .Max() + 1;
                }
                else
                {
                    return 1;
                }
            }
        }

        private static void FillIngredientsList(Reaction reaction, int requiredAmount, List<Ingredient> fullIngredientsList, IReadOnlyList<Reaction> cookBook)
        {
            foreach (var ingredient in reaction.InputChemicals)
            {
                var multiplyer = Math.Max(1, (int)Math.Ceiling((double)requiredAmount / reaction.ResultAmount));

                if (fullIngredientsList.Any(w => w.Chemical == ingredient.Chemical))
                {
                    var subIngredient = fullIngredientsList.First(w => w.Chemical == ingredient.Chemical);

                    subIngredient.Amount += ingredient.Amount * multiplyer;
                }
                else
                {
                    fullIngredientsList.Add(new Ingredient() { Chemical = ingredient.Chemical, Amount = ingredient.Amount * multiplyer });
                }
            }

            for (int complexity = reaction.Complexity; complexity > 0; complexity--)
            {
                do
                {

                    var reactions = fullIngredientsList
                        .Where(w => cookBook.Any(wc => wc.ResultChemical == w.Chemical))
                        .Select(w => cookBook.First(wc => wc.ResultChemical == w.Chemical))
                        .Where(w => w.Complexity == complexity)
                        .ToList();

                    if (reactions.Count > 0)
                    {
                        var subReaction = reactions[0];
                        var ingredient = fullIngredientsList.First(w => w.Chemical == subReaction.ResultChemical);

                        fullIngredientsList.Remove(ingredient);
                        FillIngredientsList(subReaction, ingredient.Amount, fullIngredientsList, cookBook);
                    }
                    else
                    {
                        break;
                    }

                } while (true);
                
            }
        }

        private static void Part2()
        {

        }

        class Ingredient
        {
            public string Chemical;
            public int Amount;
        }

        class Reaction
        {
            public string ResultChemical { get; set; }
            public int ResultAmount { get; set; }
            
            public int Complexity { get; set; }

            public IList<Ingredient> InputChemicals { get; }

            public Reaction(string reaction)
            {
                var numRegex = new Regex(@"-?\d+");
                var numbers = numRegex.Matches(reaction).Select(w => int.Parse(w.Value)).ToArray();

                var wordRegex = new Regex(@"-?[A-Z]+");
                var symbols = wordRegex.Matches(reaction).Select(w => w.Value).ToArray();

                if (numbers.Length != symbols.Length) throw new Exception();

                this.ResultChemical = symbols.Last();
                this.ResultAmount = numbers.Last();

                this.InputChemicals = new List<Ingredient>();

                for (int i = 0; i < numbers.Length - 1; i++)
                {
                    this.InputChemicals.Add(new Ingredient { Chemical = symbols[i], Amount = numbers[i] });
                }
            }
        }
    }
}
