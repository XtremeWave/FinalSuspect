namespace FinalSuspect.Modules.Random;

public class HashRandomWrapper : IRandom
{
    public int Next(int minValue, int maxValue)
    {
        return HashRandom.Next(minValue, maxValue);
    }

    public int Next(int maxValue)
    {
        return HashRandom.Next(maxValue);
    }
    //public uint Next() => HashRandom.Next();
    //public int FastNext(int maxValue) => HashRandom.FastNext(maxValue);
}