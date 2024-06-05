using System;
using RPN.Logic;


class Program
{

    static void Main()
    {
        string input = Console.ReadLine();
        double result = new RPNcalculator(input).Calculate();
        Console.WriteLine(result);
    }
}