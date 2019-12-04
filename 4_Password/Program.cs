using System;
using System.Collections.Generic;

namespace _4_Password
{
    class Program
    {
        static void Main(string[] args)
        {
            Part1(172930, 683082);
            Part2(172930, 683082);
        }

        private static void Part1(int low, int high)
        {
            List<int> acceptedPasswords = new List<int>();

            for (int i = low; i < high; i++)
            {
                bool onlyIncrisingDigits = HasOnlyIncreasingDigits(i);
                bool hasTwoSameAdjacentDigits = HasTwoSameAdjacentDigits(i);

                if (onlyIncrisingDigits && hasTwoSameAdjacentDigits)
                {
                    acceptedPasswords.Add(i);
                }
            }

            Console.WriteLine("Done Part 1");
            Console.WriteLine($"No Of accepted passwords: {acceptedPasswords.Count}");
        }

        private static void Part2(int low, int high)
        {
            List<int> acceptedPasswords = new List<int>();

            for (int i = low; i < high; i++)
            {
                bool onlyIncrisingDigits = HasOnlyIncreasingDigits(i);
                bool hasTwoSameAdjacentDigitsButNotPartOfAChain = HasTwoSameAdjacentDigitsButNotPartOfAChain(i);

                if (onlyIncrisingDigits && hasTwoSameAdjacentDigitsButNotPartOfAChain)
                {
                    acceptedPasswords.Add(i);
                }
            }

            Console.WriteLine("Done Part 2");
            Console.WriteLine($"No Of accepted passwords: {acceptedPasswords.Count}");
        }

        private static bool HasOnlyIncreasingDigits(int number)
        {
            int? lastDigit = null;

            var strRepresentation = number.ToString();

            for (int i = 0; i < strRepresentation.Length; i++)
            {
                var digit = int.Parse(strRepresentation.Substring(i, 1));

                if (digit < lastDigit)
                    return false;

                lastDigit = digit;
            }

            return true;
        }

        private static bool HasTwoSameAdjacentDigits(int number)
        {
            int? lastDigit = null;

            var strRepresentation = number.ToString();

            for (int i = 0; i < strRepresentation.Length; i++)
            {
                var digit = int.Parse(strRepresentation.Substring(i, 1));

                if (digit == lastDigit)
                    return true;

                lastDigit = digit;
            }

            return false;
        }

        private static bool HasTwoSameAdjacentDigitsButNotPartOfAChain(int number)
        {
            int? lastDigit = null;
            int chainLength = 0;

            var strRepresentation = number.ToString();

            for (int i = 0; i < strRepresentation.Length; i++)
            {
                var digit = int.Parse(strRepresentation.Substring(i, 1));

                if (digit == lastDigit)
                {
                    chainLength++;
                }
                else
                {
                    if (chainLength == 1)
                        return true;

                    chainLength = 0;
                }

                lastDigit = digit;
            }

            return chainLength == 1;
        }
    }
}
