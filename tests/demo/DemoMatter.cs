using Rzeka;

namespace Rzeka.Tests.Demo
{
    public class PlayerMoved : Matter
    {
        public float X { get; }
        public float Y { get; }

        public PlayerMoved(float x, float y)
        {
            X = x;
            Y = y;
        }
    }

    public class DamageTaken : Matter
    {
        public int Amount { get; }
        public string Source { get; }

        public DamageTaken(int amount, string source)
        {
            Amount = amount;
            Source = source;
        }
    }

    [HasState]
    public class HealthState : Matter
    {
        public int Current { get; }
        public int Max { get; }

        public HealthState(int current, int max)
        {
            Current = current;
            Max = max;
        }
    }

    public class PlayerDied : Matter
    {
        public string Cause { get; }

        public PlayerDied(string cause)
        {
            Cause = cause;
        }
    }
}
