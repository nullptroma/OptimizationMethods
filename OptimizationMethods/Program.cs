// See https://aka.ms/new-console-template for more information

using OptimizationMethods;
using OptimizationMethods.Models;


Console.CancelKeyPress += (_, e) => 
{
    e.Cancel = true;
    Exit();
};

while (true)
{
    Console.Clear();
    Console.WriteLine("Метод ветвей и границ");
    Console.WriteLine("Программа разработана студентом группы КТбо3-10 Пытковым Романом Евгеньевичем");
    Console.WriteLine();
    try
    {
        var n = CliTools.AskInteger("Число неизвестных: ");
        var k = CliTools.AskDoublesList("Введите коэффициенты при неизвестных у целевой функции (через пробел): ", n);

        int isMinInput;
        do
            isMinInput = CliTools.AskInteger("Что искать (0 - min, 1 - max): ");
        while (isMinInput != 0 && isMinInput != 1);
        var isMin = isMinInput == 0;

        var numLimits = CliTools.AskInteger("Число ограничений: ");
        var limits = CliTools.AskLimits("Введите ограничения", numLimits, n);
        Console.WriteLine();
        List<double>? resVector = null;
        double? res = null;
        SimplexMethod? simplexMethod = null;

        var queue = new Queue<SimplexMethod>();
        queue.Enqueue(new SimplexMethod(k, isMin, limits));

        while (queue.Count > 0)
        {
            var simplex = queue.Dequeue();
            simplex.Solve();
            var curVector = simplex.GetX(out var value);
            if (curVector.All(x => x % 1 == 0))
            {
                if (simplex.IsAcceptable == false)
                    continue;
                
                // проверка ограничений
                var isValid = true;
                foreach (var limit in limits)
                {
                    if(isValid == false)
                        break;
                    var func = 0d;
                    for (var i = 0; i < limit.K.Count; i++)
                        func += curVector[i] * limit.K[i];
                    isValid &= limit.IsLess ? func <= limit.R : func >= limit.R;
                }
                
                if (isValid && (isMin && value < res || !isMin && value > res || res == null))
                {
                    resVector = curVector;
                    res = value;
                    simplexMethod = simplex;
                }
            }
            else
            {
                var newLimits0 = simplex.OriginalLimits.ToHashSet();
                var newLimits1 = simplex.OriginalLimits.ToHashSet();
                var run0 = false;
                var run1 = false;
                for (var i = 0; i < curVector.Count; i++)
                {
                    if (curVector[i] % 1 != 0)
                    {
                        var less = Math.Floor(curVector[i]);
                        var more = Math.Ceiling(curVector[i]);
                        var limitK = Enumerable.Repeat<double>(0, i).Concat([1])
                            .Concat(Enumerable.Repeat<double>(0, n - 1 - i)).ToList();
                        var limit = new Limit
                        {
                            K = limitK,
                            IsLess = true,
                            R = less
                        };
                        if (!newLimits0.Contains(limit))
                        {
                            run0 = true;
                            newLimits0.Add(limit);
                        }
                        
                        limit = new Limit
                        {
                            K = limitK,
                            IsLess = false,
                            R = more
                        };
                        if (!newLimits1.Contains(limit))
                        {
                            run1 = true;
                            newLimits1.Add(limit);
                        }

                        break;
                    }
                }
                if(run0) queue.Enqueue(new SimplexMethod(k, isMin, newLimits0.ToList()));
                if(run1) queue.Enqueue(new SimplexMethod(k, isMin, newLimits1.ToList()));
            }
        }

        if (res == null)
        {
            Console.WriteLine("Решения не существует!");
        }
        else
        {
            Console.WriteLine($"Найдено решение в точке ({string.Join(", ", resVector!)}), оптимальное значение Z(x) = {res}");
        }
    }
    catch (ExitException)
    {
        Exit();
        break;
    }
    finally
    {
        Console.WriteLine("Для продолжения нажмите любую клавишу, для выхода - Ctrl+C");
        Console.ReadKey();
    }
}

return;

void Exit()
{
    Console.WriteLine();
    Console.WriteLine("Выход из программы");
    Environment.Exit(0);
}