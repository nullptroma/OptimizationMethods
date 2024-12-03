namespace OptimizationMethods.Models;

public class SimplexMethod
{
    private readonly int _n; // количество неизвестных
    private readonly bool _isMin; // минимизация или максимизация
    private Dictionary<RowKey, List<double>> _table = new();
    public readonly List<Limit> OriginalLimits;

    public bool IsAcceptable
    {
        get
        {
            var antiZKey = new RowKey { Type = RowKey.KeyType.AntiZ, Index = 0 };
            var notZeroKeys = new HashSet<RowKey>();
            for (var i = 0; i < _table[antiZKey].Count - 1; i++)
                if (_table[antiZKey][i] != 0)
                    notZeroKeys.Add(new RowKey
                    {
                        Type = i <= _n - 1 ? RowKey.KeyType.X : RowKey.KeyType.Additional,
                        Index = i <= _n - 1 ? i : i - _n
                    });
            return _table.Where(kp => notZeroKeys.Contains(kp.Key)).All(kp => kp.Value.Last() >= 0);
        }
    }

    public List<double> GetX(out double optimizedValue)
    {
        var antiZKey = new RowKey { Type = RowKey.KeyType.AntiZ, Index = 0 };

        var res = new double[_n];
        foreach (var kp in _table.Where(kp => kp.Key.Type == RowKey.KeyType.X))
            res[kp.Key.Index] = kp.Value.Last();
        optimizedValue = _table[antiZKey].Last();
        return res.ToList();
    }

    public SimplexMethod(List<double> k, bool isMin, List<Limit> limits)
    {
        OriginalLimits = limits;
        _n = k.Count;
        _isMin = isMin;
        _table.Add(new RowKey { Type = RowKey.KeyType.AntiZ, Index = 0 },
            k.Select(x => -x).Concat(Enumerable.Repeat(0d, limits.Count + 1)).ToList());
        for (var i = 0; i < limits.Count; i++)
        {
            var limit = limits[i];
            var row = new List<double>();
            row.AddRange(limit.K); // коэффициенты при неизвестных
            row.AddRange(Enumerable.Repeat(0d, i)); // добавляем нули перед добавочной переменной
            row.Add(limit.IsLess ? 1 : -1); // добавляем добавочную переменную: 1 или -1 
            row.AddRange(Enumerable.Repeat(0d, limits.Count - i - 1)); // добавляем нули после добавочной переменной
            row.Add(limit.R); // добавляем результат
            if (limit.IsLess == false) // если таблица небазисная, то умножаем строку на -1
                row = row.Select(x => -x).ToList();
            _table.Add(new RowKey { Type = RowKey.KeyType.Additional, Index = i }, row);
        }
    }

    public void Solve()
    {
        var kLen = _table.First().Value.Count - 1; // длина каждой строки без столбца R
        var antiZKey = new RowKey { Type = RowKey.KeyType.AntiZ, Index = 0 };

        while (_table[antiZKey].Take(kLen).Any(x => _isMin ? x > 0 : x < 0))
        {
            var antiZValues = _table[antiZKey];
            var minDivRes = double.MaxValue;

            RowKey? leaderRow = null;
            int? leaderColumn = null;
            var isAcceptable = _table.Where(kp => kp.Key.Type != RowKey.KeyType.AntiZ).All(kp => kp.Value.Last() >= 0);
            if (!isAcceptable)
            {
                // недопустимая строка 
                var unacceptable = _table
                    .First(kp => kp.Key.Type != RowKey.KeyType.AntiZ && kp.Value.Last() < 0).Value;
                int? col = null;

                for (var i = 0; i < kLen; i++)
                {
                    if (unacceptable[i] < 0)
                    {
                        var divRes = Math.Abs(antiZValues[i] / unacceptable[i]);
                        if (divRes < minDivRes)
                        {
                            minDivRes = divRes;
                            col = i;
                        }
                    }
                }

                if (col != null)
                    leaderColumn = col.Value;
            }

            if (leaderColumn == null)
            {
                var antiZWithoutLast = antiZValues.Take(kLen).ToArray();
                var value = _isMin ? double.MinValue : double.MaxValue;

                for (var col = 0; col < kLen; col++)
                {
                    var curValue = antiZValues[col];
                    var nextRowKey = new RowKey
                    {
                        Type = col <= _n - 1 ? RowKey.KeyType.X : RowKey.KeyType.Additional,
                        Index = col <= _n - 1 ? col : col - _n
                    };
                    if (curValue != 0 && _isMin ? curValue > value : curValue < value && _table.ContainsKey(nextRowKey) == false)
                    {
                        value = curValue;
                        leaderColumn = col;
                    }
                }
            }

            if (leaderColumn == null)
            {
                Console.WriteLine("Произошла внутренняя ошибка");
                break;
            }

            minDivRes = double.MaxValue;
            // перебираем все строки кроме AntiZ
            for (var row = 0; row < kLen; row++)
            {
                var rowKey = new RowKey
                {
                    Type = row <= _n - 1 ? RowKey.KeyType.X : RowKey.KeyType.Additional,
                    Index = row <= _n - 1 ? row : row - _n
                };
                if (_table.ContainsKey(rowKey) == false || _table[rowKey][leaderColumn.Value] == 0)
                    continue;
                var divRes = _table[rowKey][^1] / _table[rowKey][leaderColumn.Value];
                if (divRes < minDivRes && divRes > 0)
                {
                    minDivRes = divRes;
                    leaderRow = rowKey;
                }
            }

            if (leaderRow == null)
            {
                Console.WriteLine("Произошла внутренняя ошибка");
                break;
            }

            var leadKey = leaderRow.Value;
            var leadColumn = leaderColumn.Value;

            var leaderValue = _table[leadKey][leadColumn];

            var newTable = new Dictionary<RowKey, List<double>>();
            foreach (var kp in _table)
                newTable.Add(kp.Key, kp.Value.ToList());

            foreach (var rowKey in _table.Keys.Where(rowKey => rowKey != leadKey))
                for (var col = 0; col < _table[rowKey].Count; col++)
                    newTable[rowKey][col] -= _table[rowKey][leadColumn] * _table[leadKey][col] / leaderValue;

            for (var col = 0; col < _table[leadKey].Count; col++)
                newTable[leadKey][col] /= leaderValue;

            // Меняем ключ у строки
            var newRowKey = new RowKey
            {
                Type = leadColumn <= _n - 1 ? RowKey.KeyType.X : RowKey.KeyType.Additional,
                Index = leadColumn <= _n - 1 ? leadColumn : leadColumn - _n
            };
            newTable.Add(newRowKey, newTable[leadKey]);
            newTable.Remove(leadKey);

            _table = newTable;
        }
    }

    record struct RowKey
    {
        public enum KeyType
        {
            AntiZ,
            X,
            Additional
        }

        public readonly KeyType Type { get; init; }
        public int Index { get; init; }
    }
}