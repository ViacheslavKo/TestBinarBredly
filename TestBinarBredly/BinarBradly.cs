using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace TestBinarBredly
{
    class BinarBradly
    {
        #region Поля
        private Bitmap imageOrig;
        private Bitmap imageBinar = null;
        private double[,] imageIntegr;
        private byte[,] massByteImageOrig;
        private byte[,] massByteImageBinar = null;
        private int width;
        private int height;
        private int d = 8;
        private double procentObl = 15;
        private byte white = 1;
        private byte black = 0;
        #endregion

        #region Конструкторы
        public BinarBradly(Bitmap Original)
        {
            this.imageOrig = Original;
            this.width = imageOrig.Width;
            this.height = imageOrig.Height;
        }

        public BinarBradly(Bitmap Original, int OblastD, double Procent_srObl)
        {
            this.imageOrig = Original;
            this.width = imageOrig.Width;
            this.height = imageOrig.Height;
            this.SetOblastD = OblastD;
            this.SetProcent = Procent_srObl;
        }
        #endregion

        #region Свойства
        /// <summary>
        /// Установить D - размер области для вычисления средней яркости вокруг пикселя. (width / d)
        /// </summary>
        public int SetOblastD
        {
            set
            {
                if (value > 0 && value <= width)
                    d = width / value;
                else
                    throw new ArgumentException("Argument is not correct.");
            }
        }

        /// <summary>
        /// Порог для сравнения яркости пикселя со средней яркостью по области. (%)
        /// </summary>
        public double SetProcent
        {
            set
            {
                if (value <= 100 && value >= -100)
                    procentObl = value;
                else
                    throw new ArgumentException("Argument is not correct.");
            }
        }

        /// <summary>
        /// Получить ширину изображения.
        /// </summary>
        public int Width
        {
            get => width;
        }

        /// <summary>
        /// Получить высоту изображения.
        /// </summary>
        public int Height
        {
            get => height;
        }

        /// <summary>
        /// Получить оригинальное изображение.
        /// </summary>
        public Bitmap GetImageOriginal
        {
            get => imageOrig;
        }

        /// <summary>
        /// Получить бинаризированное изображение.
        /// Если его нет return -> null
        /// </summary>
        public Bitmap GetImageBinariz
        {
            get => imageBinar;
        }

        /// <summary>
        /// Получить массив байт бинаризированного изобр. (0 и 1 в масиве будут если была запущена функция StartBradlyBinar_0and1)
        /// Если его нет return -> null
        /// </summary>
        public byte[,] GetMassByteImageBinar
        {
            get => massByteImageBinar;
        }
        #endregion

        #region Start функции
        /// <summary>
        /// Запуск обработки фото без создания(Bitmap) обработанного фото.
        /// Массив байт будет состоять из 0 и 1
        /// </summary>
        public async Task StartBradlyBinar_0and1()
        {
            white = 1;
            black = 0;
            await Task.Run(() => InitMassiv());
            await Task.Run(() => BitmapToByteArray());
            await Task.Run(() => CreateIntegralImage());
            await Task.Run(() => BradlyBinarization());
        }

        /// <summary>
        /// Запуск обработки фото и создать фото(Bitmap).
        /// Массив байт будет состоять из 0х00 и 0xFF
        /// </summary>
        public async Task StartBradlyBinar()
        {
            white = 0xFF;
            black = 0x00;
            await Task.Run(() => InitMassiv());
            await Task.Run(() => BitmapToByteArray());
            await Task.Run(() => CreateIntegralImage());
            await Task.Run(() => BradlyBinarization());
            await Task.Run(() => ByteArrayToBitmap());
        }
        #endregion
        
        #region private функции
        private void InitMassiv()
        {
            massByteImageOrig = new byte[width, height];
            imageIntegr = new double[width + 1, height + 1];
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
            BitmapData bmpData = imageOrig.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);// PixelFormat.Format8bppIndexed
            IntPtr ptr = bmpData.Scan0;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    massByteImageOrig[j, i] = Marshal.ReadByte(ptr);
                    ptr += 0x01;
                }
            }
            imageOrig.UnlockBits(bmpData);
        }
        private void CreateIntegralImage()
        {
            for (int i = 1; i <= width; i++)
            {
                for (int j = 1; j <= height; j++)
                {
                    imageIntegr[i, j] = massByteImageOrig[i - 1, j - 1] + imageIntegr[i - 1, j] + imageIntegr[i, j - 1] - imageIntegr[i - 1, j - 1];
                }
            }
        }

        private double SrRectangleSum(int x1, int y1, int x2, int y2)
        {
            double sumLmObl = imageIntegr[x2, y2] + imageIntegr[x1, y1] - imageIntegr[x2, y1] - imageIntegr[x1, y2];
            double srObl = sumLmObl / (d * d);
            double procentOt_srObl = srObl * (procentObl / 100);
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

                    if (massByteImageOrig[i, j] < SrRectangleSum(x1, y1, x2, y2))
                    {
                        massByteImageBinar[i, j] = black;
                    }
                    else
                    {
                        massByteImageBinar[i, j] = white;
                    }
                }
            }
        }
        #endregion
    }
}
