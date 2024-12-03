namespace OptimizationMethods.Models;

public class Limit
{
    public List<double> K { get; init; } = [];
    public double R { get; init; }
    public bool IsLess { get; init; } = true;

    public override bool Equals(object? obj)
    {
        if (obj is not Limit limit)
            return false;

        return K.SequenceEqual(limit.K)
            && Math.Abs(R - limit.R) < 0.001
            && IsLess == limit.IsLess;
    }

    protected bool Equals(Limit other)
    {
        return K.Equals(other.K) && R.Equals(other.R) && IsLess == other.IsLess;
    }

    public override int GetHashCode()
    {
        var hash = K.Aggregate(19, (current, k) => current * 31 + k.GetHashCode());
        return HashCode.Combine(hash, R, IsLess);
    }
}