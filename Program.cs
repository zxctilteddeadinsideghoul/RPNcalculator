﻿using System;
using System.Diagnostics.Metrics;
using System.Globalization;

public class Token
{
    protected static readonly Dictionary<string, int> getPriority = new Dictionary<string, int>(){
        {"^", 3},
        {"*", 2},
        {"/", 2},
        {"+", 1},
        {"-", 1},
    };
}

public class Number : Token
{
    public double value;
    public Number(double value)
    {
        this.value = value;
    }
    public static Number operator +(Number left, Number right)
    {
        return new Number(left.value + right.value);
    }
    public static Number operator -(Number left, Number right)
    {
        return new Number(left.value - right.value);
    }
    public static Number operator *(Number left, Number right)
    {
        return new Number(left.value * right.value);
    }
    public static Number operator /(Number left, Number right)
    {
        return new Number(left.value / right.value);
    }
    public static Number operator ^(Number left, Number right)
    {
        return new Number(Math.Pow(left.value, right.value));
    }
}

public class Parenthesis : Token
{
    public bool isOpening;
    public Parenthesis(bool isOpening)
    {
        this.isOpening = isOpening;
    }
}

public class Operation : Token
{
    public string value;
    public int priority;
    public Operation(string value)
    {
        this.value = value;
        this.priority = getPriority[value];
    }
}

public class Function : Token
{
    public string name;
    public string firstExpression = "";
    public string secondExpression = "";
    public Function(string name, string firstExpression)
    {
        this.name = name;
        this.firstExpression = firstExpression;
    }

    public Function(string name, string firstExpression, string secondExpression)
    {
        this.name = name;
        this.firstExpression = firstExpression;
        this.secondExpression = secondExpression;
    }
}

public class Variable : Token
{

}

public static class Tokenizer
{
    public static string ClearExpression(string expression)
    {

        string[] separators = { "+", "-", "*", "/", "(", ")", "^", "|" };
        expression = expression.Replace(", ", "|");
        expression = expression.Replace(" ", "");
        foreach (string separator in separators)
        {
            expression = expression.Replace(separator, $" {separator} ");
        }
        expression = expression.Replace("  ", " ");
        return expression;
    }

    public static List<Token> Tokenize(string input)
    {
        List<Token> tokens = new List<Token>();

        input = ClearExpression(input);
        List<string> strings = new List<string>(input.Split(" "));
        string functionName = "";
        bool inFunction = false;
        bool inFirstExpression = true;
        string firstExpression = "";
        string secondExpression = "";
        int countOpeningParenthesis = 0;
        int countClosingParenthesis = 0;

        double value;

        foreach (string s in strings)
        {
            if ((s == "log" | s == "sqrt" | s == "rt" | s == "sin" | s == "cos" | s == "tg" | s == "ctg") & !inFunction)
            {
                inFunction = true;
                functionName = s;
                continue;
            }
            if (inFunction)
            {
                if (s == "(") countOpeningParenthesis++;
                if (s == ")") countClosingParenthesis++;
                if (countOpeningParenthesis == countClosingParenthesis)
                {
                    if (inFirstExpression)
                    {
                        tokens.Add(new Function(functionName, firstExpression));
                    }
                    else
                    {
                        tokens.Add(new Function(functionName, firstExpression, secondExpression));
                    }
                    functionName = "";
                    inFunction = false;
                    inFirstExpression = true;
                    firstExpression = "";
                    secondExpression = "";
                    countOpeningParenthesis = 0;
                    countClosingParenthesis = 0;
                    continue;
                }
                if (s == "|" && countOpeningParenthesis - countClosingParenthesis == 1)
                {
                    inFirstExpression = false;
                    continue;
                }
                if (inFirstExpression)
                {
                    if (s == "(" & countOpeningParenthesis == 1) continue;
                    firstExpression += s;
                }
                else
                {
                    secondExpression += s;
                }
                continue;
            }

            if (s == "(") tokens.Add(new Parenthesis(isOpening: true));
            if (s == ")") tokens.Add(new Parenthesis(isOpening: false));
            if (s == "+") tokens.Add(new Operation("+"));
            if (s == "-") tokens.Add(new Operation("-"));
            if (s == "*") tokens.Add(new Operation("*"));
            if (s == "/") tokens.Add(new Operation("/"));
            if (s == "^") tokens.Add(new Operation("^"));
            if (double.TryParse(s, out value)) tokens.Add(new Number(value));

        }
        return tokens;
    }
}

public class RPNcalculator
{
    public static List<Token> ConvertToRPN(List<Token> expression)
    {
        List<Token> expressionInRPN = new List<Token>();
        Stack<Token> stack = new Stack<Token>();

        foreach (Token token in expression)
        {
            if (token.GetType() == typeof(Number) | token.GetType() == typeof(Function))
            {
                expressionInRPN.Add(token);
            }
            if (token.GetType() == typeof(Parenthesis))
            {
                Parenthesis parenthesis = (Parenthesis)token;
                if (parenthesis.isOpening)
                {
                    stack.Push(token);
                }
                else
                {
                    while (stack.Count > 0 && stack.Peek().GetType() != typeof(Operation) && (((Parenthesis)stack.Peek()).isOpening))
                    {
                        expressionInRPN.Add(stack.Pop());
                    }
                    while (stack.Count > 0 && stack.Peek().GetType() == typeof(Operation) && (((Operation)stack.Peek()).priority >= 0))
                    {
                        expressionInRPN.Add(stack.Pop());
                    }
                    stack.Pop();
                }
            }
            if (token.GetType() == typeof(Operation))
            {
                int priority;
                if (stack.Count > 0 && stack.Peek().GetType() == typeof(Operation))
                {
                    priority = ((Operation)stack.Peek()).priority;
                }
                else
                {
                    priority = 0;
                }
                while (stack.Count > 0 && (priority >= ((Operation)token).priority))
                {
                    expressionInRPN.Add(stack.Pop());
                }
                stack.Push(token);
            }
        }
        foreach (Token token in stack)
        {
            expressionInRPN.Add(token);
        }

        return expressionInRPN;
    }

    private static Number CalculateOperation(Operation op, Number first, Number second)
    {
        Number result = new Number(0);
        switch (op.value)
        {
            case "+": result = first + second; break;
            case "-": result = first - second; break;
            case "*": result = first * second; break;
            case "/": result = first / second; break;
            case "^": result = first ^ second; break;
        }
        return result;
    }

    private static Number CalculateFunction(Function fn)
    {
        double result = 0;
        List<Token> tokens = RPNcalculator.ConvertToRPN(Tokenizer.Tokenize(fn.firstExpression));
        double first = RPNcalculator.CalculateRPNExpression(tokens);
        tokens = RPNcalculator.ConvertToRPN(Tokenizer.Tokenize(fn.secondExpression));
        double second = RPNcalculator.CalculateRPNExpression(tokens);

        switch (fn.name)
        {
            case "log":
                result = Math.Log(second, first);
                break;
            case "rt":
                result = Math.Pow(second, 1 / first);
                break;
            case "sqrt":
                result = Math.Sqrt(first);
                break;
            case "sin":
                result = Math.Sin(first);
                break;
            case "cos":
                result = Math.Cos(first);
                break;
        }
        return new Number(result);
    }

    public static double CalculateRPNExpression(List<Token> RPNexpression)
    {
        int counter = 0;
        Stack<Number> stack = new Stack<Number>();
        foreach (Token token in RPNexpression)
        {
            if (token.GetType() == typeof(Number) | token.GetType() == typeof(Function))
            {
                if (token.GetType() == typeof(Number))
                {
                    stack.Push((Number)token);
                }
                else
                {
                    stack.Push(CalculateFunction((Function)token));
                }
            }
            else if (token.GetType() == typeof(Operation))
            {
                counter++;
                Number second, first;
                if (stack.Count > 0) second = stack.Pop(); else second = new Number(0);
                if (stack.Count > 0) first = stack.Pop(); else first = new Number(0);
                stack.Push(CalculateOperation((Operation)token, first, second));
            }
        }
        return stack.Count > 0 ? stack.Pop().value : 0;
    }
}


class Program
{

    static void Main()
    {


    }
}