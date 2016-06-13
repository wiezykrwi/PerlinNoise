using LandscapeGenerator.Data;
using LandscapeGenerator.Engine.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandscapeGenerator.Engine
{
    internal class PerlinNoiseEngine
    {
        private const int PermutationLength = 256;

        private readonly IRandomNumberFactory _randomNumberFactory;

        private int[] _permutation;
        private Vector2[] _gradients;

        internal PerlinNoiseEngine() : this(new DefaultRandomNumberFactory())
        {
        }

        internal PerlinNoiseEngine(IRandomNumberFactory randomNumberFactory)
        {
            _randomNumberFactory = randomNumberFactory;
        }

        internal void InitializePermutation()
        {
            _permutation = Enumerable.Range(0, PermutationLength).ToArray();

            /// shuffle the array
            for (var i = 0; i < PermutationLength; i++)
            {
                var source = _randomNumberFactory.Next(PermutationLength);

                var t = _permutation[i];
                _permutation[i] = _permutation[source];
                _permutation[source] = t;
            }
        }

        internal void InitializeGradients()
        {
            _gradients = new Vector2[256];

            for (var i = 0; i < _gradients.Length; i++)
            {
                Vector2 gradient;

                do
                {
                    gradient = new Vector2((float)(_randomNumberFactory.NextDouble() * 2 - 1), (float)(_randomNumberFactory.NextDouble() * 2 - 1));
                }
                while (gradient.LengthSquared() >= 1);

                gradient.Normalize();

                _gradients[i] = gradient;
            }
        }

        internal byte[] GenerateNoiseMap(int width, int height, int octaves = 8, float frequency = 0.5f, float amplitude = 1f)
        {
            var noisemapBase = new float[width * height];

            /// track min and max noise value. Used to normalize the result to the 0 to 1.0 range.
            var min = float.MaxValue;
            var max = float.MinValue;

            for (var octave = 0; octave < octaves; octave++)
            {
                /// parallel loop - easy and fast.
                Parallel.For(0
                    , width * height
                    , (offset) =>
                    {
                        var i = offset % width;
                        var j = offset / width;
                        var noise = Noise(i * frequency * 1f / width, j * frequency * 1f / height);
                        noise = noisemapBase[j * width + i] += noise * amplitude;

                        min = Math.Min(min, noise);
                        max = Math.Max(max, noise);

                    }
                );

                frequency *= 2;
                amplitude /= 2;
            }

            var noisemap = new byte[width * height];

            for (int i = 0; i < width * height; i++)
            {
                var f = noisemapBase[i];
                var result = (f - min) / (max - min);
                byte value = (byte)(result * 255);
                noisemap[i] = value;
            }

            return noisemap;
        }

        private float Noise(float x, float y)
        {
            var cell = new Vector2((float)Math.Floor(x), (float)Math.Floor(y));

            var total = 0f;

            var corners = new[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0), new Vector2(1, 1) };

            foreach (var n in corners)
            {
                var ij = cell.Add(n);
                var uv = new Vector2(x - ij.X, y - ij.Y);

                var index = _permutation[(int)ij.X % _permutation.Length];
                index = _permutation[(index + (int)ij.Y) % _permutation.Length];

                var grad = _gradients[index % _gradients.Length];

                total += Q(uv.X, uv.Y) * Vector2.Dot(grad, uv);
            }

            return Math.Max(Math.Min(total, 1f), -1f);
        }

        private static float Drop(float t)
        {
            t = Math.Abs(t);
            return 1f - t * t * t * (t * (t * 6 - 15) + 10);
        }

        private static float Q(float u, float v)
        {
            return Drop(u) * Drop(v);
        }
    }
}
