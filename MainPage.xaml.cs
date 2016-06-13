using System;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.Graphics.DirectX;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using LandscapeGenerator.Engine;

namespace LandscapeGenerator
{
    public sealed partial class MainPage : Page
    {
        private readonly PerlinNoiseEngine _perlinNoiseEngine;

        private ICanvasImage _noisebitmap;
        private ICanvasImage _shapebitmap;
        private ICanvasImage _heightbitmap;
        private ICanvasImage _finalbitmap;

        public MainPage()
        {
            InitializeComponent();

            _perlinNoiseEngine = new PerlinNoiseEngine();
            _perlinNoiseEngine.InitializeGradients();
            _perlinNoiseEngine.InitializePermutation();
        }

        private void GenerateGradientButtonClicked(object sender, RoutedEventArgs e)
        {
            _perlinNoiseEngine.InitializeGradients();
        }

        private void GeneratePermutationsButtonClicked(object sender, RoutedEventArgs e)
        {
            _perlinNoiseEngine.InitializePermutation();
        }

        private async void GenerateButtonClicked(object sender, RoutedEventArgs e)
        {
            if (AutoGradientCheckBox.IsChecked.GetValueOrDefault())
            {
                _perlinNoiseEngine.InitializeGradients();
            }

            if (AutoPermutationsCheckBox.IsChecked.GetValueOrDefault())
            {
                _perlinNoiseEngine.InitializePermutation();
            }

            var actualWidth = (int)NoiseCanvasControl.ActualWidth;
            var actualHeight = (int)NoiseCanvasControl.ActualHeight;

            var octaves = (int)OctavesSlider.Value;
            var frequency = (float)(FrequencySlider.Value / 1000);
            var amplitude = (float)(AmplitudeSlider.Value / 1000);

            var noisemap = await Task.Run(() => _perlinNoiseEngine.GenerateNoiseMap(actualWidth, actualHeight, octaves, frequency, amplitude));

            var totalSize = actualWidth * actualHeight;
            var noisebytes = new byte[totalSize * 4];
            for (int i = 0; i < totalSize; i++)
            {
                noisebytes[i * 4] = noisemap[i];
                noisebytes[i * 4 + 1] = noisemap[i];
                noisebytes[i * 4 + 2] = noisemap[i];
                noisebytes[i * 4 + 3] = 255;
            }

            _noisebitmap = CanvasBitmap.CreateFromBytes(NoiseCanvasControl, noisebytes, actualWidth, actualHeight, DirectXPixelFormat.B8G8R8A8UIntNormalized);
            
            var shapemap = new byte[totalSize];
            int centerX = actualWidth / 2;
            int centerY = actualHeight / 2;

            int radius = (int)((actualWidth / 2) * .8);

            for (int y = 0; y < actualHeight; y++)
            {
                for (int x = 0; x < actualWidth; x++)
                {
                    float distance = (float)Math.Sqrt(Math.Pow(x - centerX, 2) + Math.Pow(y - centerY, 2));
                    if (distance > radius)
                    {
                        continue;
                    }

                    float part = distance / radius;
                    byte value = (byte)(255 * (1 - part));
                    shapemap[y * actualWidth + x] = value;
                }
            }

            var shapebytes = new byte[totalSize * 4];
            for (int i = 0; i < totalSize; i++)
            {
                shapebytes[i * 4] = shapemap[i];
                shapebytes[i * 4 + 1] = shapemap[i];
                shapebytes[i * 4 + 2] = shapemap[i];
                shapebytes[i * 4 + 3] = 255;
            }

            _shapebitmap = CanvasBitmap.CreateFromBytes(ShapeCanvasControl, shapebytes, actualWidth, actualHeight, DirectXPixelFormat.B8G8R8A8UIntNormalized);

            var heightbytes = new byte[totalSize * 4];
            for (int i = 0; i < totalSize; i++)
            {
                byte first = noisemap[i];
                byte second = shapemap[i];
                int temp = (first + second) / 2;

                byte value = (byte) temp;

                heightbytes[i * 4] = value;
                heightbytes[i * 4 + 1] = value;
                heightbytes[i * 4 + 2] = value;
                heightbytes[i * 4 + 3] = 255;
            }

            _heightbitmap = CanvasBitmap.CreateFromBytes(HeightCanvasControl, heightbytes, actualWidth, actualHeight, DirectXPixelFormat.B8G8R8A8UIntNormalized);

            var bytes = new byte[totalSize * 4];
            for (int i = 0; i < totalSize; i++)
            {
                int value = (noisemap[i] + shapemap[i]) / 2;
                int index = i * 4;

                if (value < 100)
                {
                    // dark blue - ocean
                    bytes[index] = 104;
                    bytes[index + 1] = 34;
                    bytes[index + 2] = 26;
                    bytes[index + 3] = 255;
                }
                else if (value < 128)
                {
                    // lighter blue - shallow
                    bytes[index] = 212;
                    bytes[index + 1] = 118;
                    bytes[index + 2] = 58;
                    bytes[index + 3] = 255;
                }
                else if (value < 140)
                {
                    // yellowish - coast
                    bytes[index] = 145;
                    bytes[index + 1] = 231;
                    bytes[index + 2] = 255;
                    bytes[index + 3] = 255;
                }
                else if (value < 180)
                {
                    // green - grass/forest
                    bytes[index] = 43;
                    bytes[index + 1] = 149;
                    bytes[index + 2] = 63;
                    bytes[index + 3] = 255;
                }
                else if (value < 210)
                {
                    // gray - mountain
                    bytes[index] = 100;
                    bytes[index + 1] = 100;
                    bytes[index + 2] = 100;
                    bytes[index + 3] = 255;
                }
                else
                {
                    // white - snow
                    bytes[index] = 255;
                    bytes[index + 1] = 255;
                    bytes[index + 2] = 255;
                    bytes[index + 3] = 255;
                }
            }

            _finalbitmap = CanvasBitmap.CreateFromBytes(FinalCanvasControl, bytes, actualWidth, actualHeight, DirectXPixelFormat.B8G8R8A8UIntNormalized);

            NoiseCanvasControl.Invalidate();
            ShapeCanvasControl.Invalidate();
            HeightCanvasControl.Invalidate();
            FinalCanvasControl.Invalidate();
        }

        private void NoiseCanvasControl_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            if (_noisebitmap == null)
            {
                return;
            }

            args.DrawingSession.DrawImage(_noisebitmap);
        }

        private void ShapeCanvasControl_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            if (_shapebitmap == null)
            {
                return;
            }

            args.DrawingSession.DrawImage(_shapebitmap);
        }

        private void HeightCanvasControl_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            if (_heightbitmap == null)
            {
                return;
            }

            args.DrawingSession.DrawImage(_heightbitmap);
        }

        private void FinalCanvasControl_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            if (_finalbitmap == null)
            {
                return;
            }

            args.DrawingSession.DrawImage(_finalbitmap);
        }
    }
}
