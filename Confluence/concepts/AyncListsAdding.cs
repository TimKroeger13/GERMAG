using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class Program
{
    private static Random rnd = new Random();

    public static async Task Main()
    {
        List<double?> InputListOfNumber = GenerateInputList(10);
        List<double?> OutputListOfNumber = await ProcessInputListAsync(InputListOfNumber);

        // Output the modified list
        foreach (var number in OutputListOfNumber)
        {
            Console.WriteLine(number);
        }
    }

    private static List<double?> GenerateInputList(int count)
    {
        List<double?> inputList = new List<double?>();
        for (int i = 0; i < count; i++)
        {
            inputList.Add((double)i + 1); // Adding non-null values from 1 to count
        }
        return inputList;
    }

    private static async Task<List<double?>> ProcessInputListAsync(List<double?> inputList)
    {
        List<Task<List<double?>>> tasks = new List<Task<List<double?>>>();

        foreach (var number in inputList)
        {
            tasks.Add(SplitNumberRandomAsync(number.Value));
        }

        List<double?>[] results = await Task.WhenAll(tasks);

        List<double?> outputList = new List<double?>();
        foreach (var result in results)
        {
            outputList.AddRange(result);
        }

        return outputList;
    }

    private static async Task<List<double?>> SplitNumberRandomAsync(double number)
    {
        int dice = rnd.Next(1, 7);
        await Task.Delay(dice * 1000); // Asynchronously delay

        List<double?> sourceAndDoubleNumber = new List<double?>();
        sourceAndDoubleNumber.Add(number);
        sourceAndDoubleNumber.Add(number + dice);

        return sourceAndDoubleNumber;
    }
}



