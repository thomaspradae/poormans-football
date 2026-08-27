using System;
using System.Text;

namespace FootballWorldLab.Core.Rng
{
    /// <summary>
    /// A deterministic, seedable pseudo-random number generator implementation
    /// using standard PCG32 algorithm for reliable cross-platform simulation reproducibility.
    /// </summary>
    public sealed class SeededRandom
    {
        private ulong _state;
        private ulong _inc;

        public ulong Seed { get; }

        public SeededRandom(ulong seed, ulong stream = 54u)
        {
            Seed = seed;
            _state = 0U;
            _inc = (stream << 1) | 1u;
            NextUInt();
            _state += seed;
            NextUInt();
        }

        public SeededRandom CreateChild(string SubsystemName)
        {
            // String.GetHashCode is process-randomized in .NET; use a stable
            // UTF-8 FNV-1a hash so identical seeds reproduce across runs.
            ulong hash = 14695981039346656037UL;
            foreach (byte value in Encoding.UTF8.GetBytes(SubsystemName))
            {
                hash ^= value;
                hash *= 1099511628211UL;
            }
            ulong childSeed = NextUInt64() ^ hash;
            return new SeededRandom(childSeed, _inc >> 1);
        }

        public uint NextUInt()
        {
            ulong oldstate = _state;
            _state = unchecked(oldstate * 6364136223846793005UL + _inc);
            uint xorshifted = (uint)unchecked((((oldstate >> 18) ^ oldstate) >> 27));
            int rot = (int)(oldstate >> 59);
            return unchecked((xorshifted >> rot) | (xorshifted << ((-rot) & 31)));
        }

        public ulong NextUInt64()
        {
            ulong high = NextUInt();
            ulong low = NextUInt();
            return (high << 32) | low;
        }

        public int NextInt(int minValue, int maxValue)
        {
            if (minValue >= maxValue)
                throw new ArgumentOutOfRangeException(nameof(minValue), "minValue must be less than maxValue.");

            uint range = (uint)(maxValue - minValue);
            uint threshold = (uint)(-range) % range;

            while (true)
            {
                uint r = NextUInt();
                if (r >= threshold)
                    return minValue + (int)(r % range);
            }
        }

        public int NextInt(int maxValue) => NextInt(0, maxValue);

        public double NextDouble()
        {
            return (NextUInt() >> 11) * (1.0 / (1 << 21));
        }

        public float NextFloat()
        {
            return (float)NextDouble();
        }

        public bool NextBool(double probability = 0.5)
        {
            return NextDouble() < probability;
        }
    }
}
