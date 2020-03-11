using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace TestBinarBredly
{
    class BinarBradly
    {
        private Bitmap imageOriginal;
        private Bitmap imageBinar;
        private double[,] imageIntegral;
        private byte[,] massByteImageBinar;
        private byte[,] massByteImageOriginal;
        private int width;
        private int height;
        private int d = 8;
        private double procent_srObl = 15;

        public BinarBradly(Bitmap Original)
        {
            this.imageOriginal = Original;
            this.width = imageOriginal.Width;
            this.height = imageOriginal.Height;
        }

        public BinarBradly(Bitmap Original, int OblastD, double Procent_srObl)
        {
            this.imageOriginal = Original;
            this.width = imageOriginal.Width;
            this.height = imageOriginal.Height;
            this.OblastD = OblastD;
            this.Procent = Procent_srObl;
        }

        public int OblastD
        {
            set
            {
                if (value != 0)
                    d = width / value;
                else
                    throw new ArgumentException("Argument is not correct.");
            }
        }

        public double Procent
        {
            set
            {
                if (value <= 100 && value >= 0)
                    procent_srObl = value;
                else
                    throw new ArgumentException("Argument is not correct.");
            }
        }

        public int Width
        {
            get => width;
        }

        public int Height
        {
            get => height;
        }

        public Bitmap GetImageOriginal
        {
            get => imageOriginal;
        }

        public Bitmap GetImageBinariz
        {
            get => imageBinar;
        }

        public async Task StartBradlyBinar()
        {
            await Task.Run(() => InitMassiv());
            await Task.Run(() => BitmapToByteArray());
            await Task.Run(() => CreateIntegralImage());
            await Task.Run(() => BradlyBinarization());
            await Task.Run(() => ByteArrayToBitmap());
        }

        private void InitMassiv()
        {
            massByteImageOriginal = new byte[width, height];
            imageIntegral = new double[width + 1, height + 1];
            massByteImageBinar = new byte[width, height];
        }

        private void ByteArrayToBitmap()
        {
            imageBinar = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            BitmapData bmpData = imageBinar.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            IntPtr ptr = bmpData.Scan0;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    Marshal.WriteByte(ptr, massByteImageBinar[j, i]);
                    ptr += 0x01;
                }
            }
            imageBinar.UnlockBits(bmpData);
        }

        private void BitmapToByteArray()
        {
            BitmapData bmpData = imageOriginal.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);// PixelFormat.Format8bppIndexed
            IntPtr ptr = bmpData.Scan0;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    massByteImageOriginal[j, i] = Marshal.ReadByte(ptr);
                    ptr += 0x01;
                }
            }
            imageOriginal.UnlockBits(bmpData);
        }
        private void CreateIntegralImage()
        {
            for (int i = 1; i <= width; i++)
            {
                for (int j = 1; j <= height; j++)
                {
                    imageIntegral[i, j] = massByteImageOriginal[i - 1, j - 1] + imageIntegral[i - 1, j] + imageIntegral[i, j - 1] - imageIntegral[i - 1, j - 1];
                }
            }
        }

        private double SrRectangleSum(int x1, int y1, int x2, int y2)
        {
            double sumLmObl = imageIntegral[x2, y2] + imageIntegral[x1, y1] - imageIntegral[x2, y1] - imageIntegral[x1, y2];
            double srObl = sumLmObl / (d * d);
            double procentOt_srObl = srObl * (procent_srObl / 100);
            return srObl - procentOt_srObl;
        }

        private void BradlyBinarization()
        {
            int d2 = d / 2;
            for (int i = 0; i < width; i++)
            {
                int x1 = i - d2;
                int x2 = i + d2;
                if (x1 < 0)
                    x1 = 0;
                if (x2 >= width)
                    x2 = width - 1;

                for (int j = 0; j < height; j++)
                {
                    int y1 = j - d2;
                    int y2 = j + d2;
                    if (y1 < 0)
                        y1 = 0;
                    if (y2 >= height)
                        y2 = height - 1;

                    if (massByteImageOriginal[i, j] < SrRectangleSum(x1, y1, x2, y2))
                    {
                        massByteImageBinar[i, j] = 0x00;// 0x00 Black
                    }
                    else
                    {
                        massByteImageBinar[i, j] = 0xFF;// 0xFF White
                    }
                }
            }
        }

    }
}
