namespace LandscapeGenerator.Engine.Interfaces
{
    internal interface IRandomNumberFactory
    {
        /// <summary>
        /// Generate a random integer value.
        /// </summary>
        /// <param name="maxValue">Exclusive upper bound.</param>
        /// <returns>A positive integer.</returns>
        int Next(int maxValue);

        /// <summary>
        /// Generate a random double value.
        /// </summary>
        /// <returns>A double value  between 0 and 1.</returns>
        double NextDouble();
    }
}