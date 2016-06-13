using LandscapeGenerator.Engine.Interfaces;

using System;

namespace LandscapeGenerator.Engine
{
    internal class DefaultRandomNumberFactory : IRandomNumberFactory
    {
        private readonly Random _random;

        public DefaultRandomNumberFactory()
        {
            _random = new Random();
        }

        public DefaultRandomNumberFactory(int seed)
        {
            _random = new Random(seed);
        }

        public int Next(int maxValue)
        {
            return _random.Next(maxValue);
        }

        public double NextDouble()
        {
            return _random.NextDouble();
        }
    }
}
