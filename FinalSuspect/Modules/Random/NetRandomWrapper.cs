namespace FinalSuspect.Modules.Random;

public class NetRandomWrapper : IRandom
{
    public System.Random wrapping;

    public NetRandomWrapper() : this(new System.Random())
    { }
    public NetRandomWrapper(int seed) : this(new System.Random(seed))
    { }
    public NetRandomWrapper(System.Random instance)
    {
        wrapping = instance;
    }

    public int Next(int minValue, int maxValue) => wrapping.Next(minValue, maxValue);
    public int Next(int maxValue) => wrapping.Next(maxValue);
    public int Next() => wrapping.Next();
}