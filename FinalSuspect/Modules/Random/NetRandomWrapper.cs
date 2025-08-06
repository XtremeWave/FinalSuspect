namespace FinalSuspect.Modules.Random;

public class NetRandomWrapper(System.Random instance) : IRandom
{
    public NetRandomWrapper() : this(new System.Random())
    {
    }

    public NetRandomWrapper(int seed) : this(new System.Random(seed))
    {
    }

    public int Next(int minValue, int maxValue)
    {
        return instance.Next(minValue, maxValue);
    }

    public int Next(int maxValue)
    {
        return instance.Next(maxValue);
    }

    public int Next()
    {
        return instance.Next();
    }
}