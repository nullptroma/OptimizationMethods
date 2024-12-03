using OptimizationMethods.Models;

namespace OptimizationMethods;

public static class CliTools
{
    public static int AskInteger(string message)
    {
        Console.Write(message);
        return int.Parse(Console.ReadLine() ?? throw new ExitException());
    }
    
    public static char AskChar(string message)
    {
        Console.Write(message);
        return (Console.ReadLine() ?? throw new ExitException())[0];
    }
    
    public static List<double> AskDoublesList(string message, int n)
    {
        Console.Write(message);
        var res = new List<double>();
        while (res.Count != n)
        {
            var str = Console.ReadLine() ?? throw new ExitException();
            var parts = str.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                res.Add(double.Parse(part));
                if(res.Count == n)
                    break;
            }
        }

        return res;
    }
    
    public static List<Limit> AskLimits(string message, int numLimits, int n)
    {
        Console.WriteLine(message);
        var limits = new List<Limit>();
        for (var i = 0; i < numLimits; i++)
        {            
            Console.WriteLine($"Ограничение {i + 1}");
            var k = AskDoublesList("Введите коэффициенты при неизвестных (через пробел): ", n);
            char ch;
            do
                ch = AskChar("Введите знак (< - <=, > - >=): ");
            while ("<>".Contains(ch) == false);
            var r = AskInteger("Введите свободный член: ");
            var limit = new Limit
            {
                K = k,
                IsLess = ch == '<',
                R = r
            };
            limits.Add(limit);
        }

        return limits;
    }
}