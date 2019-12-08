using System;
using SantasToolbox;
using System.Linq;
using System.Collections.Generic;

namespace _8_Image
{
    class Program
    {
        static void Main(string[] args)
        {
            var inputParser = new SingleLineStringInputParser<int>(int.TryParse, w => w.Select(w => new string(w, 1)).ToArray());
            using var inputProvider = new InputProvider<int>("Input.txt", inputParser.GetValue);

            //Part1(inputProvider);
            //Part1(new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2 }.GetEnumerator());

            inputProvider.Reset();

            Part2(inputProvider);
            //Part2(new List<int> { 0, 2, 2, 2, 1, 1, 2, 2, 2, 2, 1, 2, 0, 0, 0, 0 }.GetEnumerator());
        }

        private static void Part1(IEnumerator<int> inputProvider)
        {
            int width = 25;
            int height = 6;

            var layers = ParseLayers(inputProvider, width, height);

            var zeroes = layers.Select(w => w.FindAll(w => w == 0).Count).ToList();

            var index = zeroes.FindIndex(w => w == zeroes.Min());

            var ones = layers[index].Count(w => w == 1);
            var twos = layers[index].Count(w => w == 2);

            Console.WriteLine($"Part 1: ones: {ones} twos: {twos}");
        }

        private static void Part2(IEnumerator<int> inputProvider)
        {
            int width = 25;
            int height = 6;

            var layers = ParseLayers(inputProvider, width, height);

            int[,] image = new int[width, height];

            for (int y = 0; y < height; y++) 
            {
                for (int x = 0; x < width; x++)
                {
                    var indexInLayer = y * width + x;
                    var pixels = layers.Select(w => w[indexInLayer]).ToList();

                    int pixel = -1;
                    
                    // 2 is transparent
                    
                    foreach (var color in pixels)
                    {
                        if (color == 2)
                            continue;

                        pixel = color;
                        break;
                    }

                    image[x, y] = pixel;
                }
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var color = image[x, y];

                    ConsoleColor colorToPrint = color switch
                    {
                        0 => ConsoleColor.Black,
                        1 => ConsoleColor.White,
                        _ => throw new Exception()
                    };

                    Console.BackgroundColor = colorToPrint;
                    Console.Write(' ');
                }
                Console.Write(Environment.NewLine);
            }

        }

        private static List<List<int>> ParseLayers(IEnumerator<int> inputProvider, int width, int height)
        {
            var layers = new List<List<int>>();

            List<int>? currentLayer;

            for (int layer = 0; ; layer++)
            {
                currentLayer = null;

                for (int row = 0; row < height; row++)
                {
                    for (int column = 0; column < width; column++)
                    {
                        if (!inputProvider.MoveNext())
                            return layers;

                        if (currentLayer == null)
                        {
                            currentLayer = new List<int>();
                            layers.Add(currentLayer);
                        }

                        currentLayer.Add(inputProvider.Current);
                    }
                }
            }
        }
    }
}
