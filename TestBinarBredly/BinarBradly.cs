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
        private Bitmap imageOrig = null;
        private Bitmap imageBinar = null;
        private double[,] imageIntegrOrig = null;
        private int[,] imageIntegrBinar = null;
        private double[,] massByteImageOrig = null;
        private byte[,] massByteImageBinar = null;
        private int width = -1;
        private int height = -1;
        private int d = 8;
        private double procentObl = 15;
        private byte white = 1;
        private byte black = 0;
        //public static List<SettingThreadCam> settingList = new List<SettingThreadCam>();
        #endregion

        #region Конструкторы
        /// <summary>
        /// Инициализирует пустой экземпляр класса.
        /// </summary>
        public BinarBradly()
        {
            imageOrig = null;
            imageBinar = null;
            massByteImageBinar = null;
            imageIntegrBinar = null;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса с фото.
        /// </summary>
        public BinarBradly(Bitmap Original)
        {
            this.imageOrig = Original;
            this.width = imageOrig.Width;
            this.height = imageOrig.Height;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса с фото и устанавливает параметры D и %.
        /// </summary>
        public BinarBradly(Bitmap Original, int OblastD, double Procent_srObl)
        {
            this.imageOrig = Original;
            this.width = imageOrig.Width;
            this.height = imageOrig.Height;
            SetOblastD(OblastD);
            SetProcent(Procent_srObl);
        }
        #endregion

        #region Свойства
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
        /// Если его нет return -> null
        /// </summary>
        public Bitmap GetImageOrig
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

        /// <summary>
        /// Получить массив бинаризированного интегрированного изобр.
        /// Если его нет return -> null
        /// Размер [height + 1, width + 1]. Полезные данные начинаются с позиции [1, 1]
        /// </summary>
        public int[,] GetImageIntegrBinar
        {
            get => imageIntegrBinar;
        }
        #endregion

        #region Set-функции
        /// <summary>
        /// Загрузить новое оригинальное изображение.
        /// </summary>
        public void SetImageOrig(Bitmap value)
        {
            this.imageOrig = value;
            this.width = imageOrig.Width;
            this.height = imageOrig.Height;

            imageBinar = null;
            imageIntegrOrig = null;
            massByteImageOrig = null;
            massByteImageBinar = null;
            imageIntegrBinar = null;
        }

        /// <summary>
        /// Установить D - размер области для вычисления средней яркости вокруг пикселя. (width / d)
        /// </summary>
        public void SetOblastD(int value)
        {
            if (value > 0 && value <= width)
                d = width / value;
            else
                throw new ArgumentException("Argument is not correct.");
        }

        /// <summary>
        /// Порог для сравнения яркости пикселя со средней яркостью по области. (%)
        /// </summary>
        public void SetProcent(double value)
        {
            if (value <= 100 && value >= -100)
                procentObl = value;
            else
                throw new ArgumentException("Argument is not correct.");
        }
        #endregion

        #region Start функции
        /// <summary>
        /// Запуск обработки фото без создания(Bitmap) обработанного фото.
        /// Массив байт будет состоять из 0 и 1
        /// </summary>
        public async Task StartBradlyBinar_0and1()
        {
            if (imageOrig == null) { throw new ArgumentException("Image not found."); }
            white = 1;
            black = 0;
            await Task.Run(() => InitMassiv());
            await Task.Run(() => BitmapToByteArray());
            await Task.Run(() => CreateIntegralImage());
            await Task.Run(() => BradlyBinarization());
            imageIntegrOrig = null;
            massByteImageOrig = null;
        }

        /// <summary>
        /// Запуск обработки фото и создать фото(Bitmap).
        /// Массив байт будет состоять из 0х00 и 0xFF (0 и 255)
        /// </summary>
        public async Task StartBradlyBinar()
        {
            if (imageOrig == null) { throw new ArgumentException("Image not found."); }
            white = 0xFF;
            black = 0x00;
            await Task.Run(() => InitMassiv());
            await Task.Run(() => BitmapToByteArray());
            await Task.Run(() => CreateIntegralImage());
            await Task.Run(() => BradlyBinarization());
            await Task.Run(() => ByteArrayToBitmap());
            imageIntegrOrig = null;
            massByteImageOrig = null;
        }
        #endregion

        #region private функции
        private void InitMassiv()
        {
            imageIntegrOrig = new double[height, width];
            imageIntegrBinar = new int[height, width];
            massByteImageOrig = new double[height, width];
            massByteImageBinar = new byte[height, width];
        }

        private void ByteArrayToBitmap()
        {
            imageBinar = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            BitmapData bmpData = imageBinar.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            IntPtr ptr;
            IntPtr ptrConst = bmpData.Scan0;
            int StrideWidth = bmpData.Stride;

            for (int i = 0; i < height; i++)
            {
                ptr = ptrConst + i * StrideWidth;
                for (int j = 0; j < width; j++)
                {
                    Marshal.WriteByte(ptr, massByteImageBinar[i, j]);
                    ptr += 0x01;
                }
            }
            imageBinar.UnlockBits(bmpData);
        }

        private void BitmapToByteArray()
        {
            BitmapData bmpData = imageOrig.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, imageOrig.PixelFormat);
            IntPtr ptr;
            IntPtr ptrConst = bmpData.Scan0;
            int StrideWidth = bmpData.Stride;

            switch (imageOrig.PixelFormat)
            {
                case PixelFormat.Format8bppIndexed:
                    {
                        for (int i = 0; i < height; i++)
                        {
                            ptr = ptrConst + i * StrideWidth;
                            for (int j = 0; j < width; j++)
                            {
                                massByteImageOrig[i, j] = Marshal.ReadByte(ptr);
                                ptr += 0x01;
                            }
                        }
                    }
                    break;

                case PixelFormat.Format24bppRgb:
                    {
                        for (int i = 0; i < height; i++)
                        {
                            ptr = ptrConst + i * StrideWidth;
                            for (int j = 0; j < width; j++)
                            {
                                double B = Marshal.ReadByte(ptr);
                                ptr += 0x01;
                                double G = Marshal.ReadByte(ptr);
                                ptr += 0x01;
                                double R = Marshal.ReadByte(ptr);
                                ptr += 0x01;

                                massByteImageOrig[i, j] = 0.2126 * R + 0.7152 * G + 0.0722 * B;
                            }
                        }
                    }
                    break;
                default:
                    {
                        throw new ArgumentException("PixelFormat is not correct.");
                    }
            }
            imageOrig.UnlockBits(bmpData);
        }

        private void CreateIntegralImage()
        {
            imageIntegrOrig[0, 0] = massByteImageOrig[0, 0];//Самая быстрая реализация
            for (int i = 1; i < width; i++)
            {
                imageIntegrOrig[0, i] = imageIntegrOrig[0, i - 1] + massByteImageOrig[0, i];
            }

            for (int i = 1; i < height; i++)
            {
                imageIntegrOrig[i, 0] = imageIntegrOrig[i - 1, 0] + massByteImageOrig[i, 0];
                for (int j = 1; j < width; j++)
                {
                    imageIntegrOrig[i, j] = massByteImageOrig[i, j] + imageIntegrOrig[i - 1, j] + imageIntegrOrig[i, j - 1] - imageIntegrOrig[i - 1, j - 1];
                }
            }

            #region еще варианты реализации, но они медленнее немного
            //for (int i = 0; i < height; i++)// такая реализация медленее примерно на ~0.04s
            //{
            //    for (int j = 0; j < width; j++)
            //    {
            //        if (i == 0 && j == 0)
            //            imageIntegrOrig[0, 0] = massByteImageOrig[0, 0];
            //        else if (i == 0)
            //            imageIntegrOrig[i, j] = massByteImageOrig[i, j] + imageIntegrOrig[0, j] + imageIntegrOrig[i, j - 1] - imageIntegrOrig[0, j - 1];
            //        else if (j == 0)
            //            imageIntegrOrig[i, j] = massByteImageOrig[i, j] + imageIntegrOrig[i - 1, j] + imageIntegrOrig[i, 0] - imageIntegrOrig[i - 1, 0];
            //        else
            //            imageIntegrOrig[i, j] = massByteImageOrig[i, j] + imageIntegrOrig[i - 1, j] + imageIntegrOrig[i, j - 1] - imageIntegrOrig[i - 1, j - 1];
            //    }
            //}

            //for (int i = 0; i < height; i++)// такая реализация быстрее примерно на ~0.005s чем более медленная версия выше
            //{
            //    for (int j = 0; j < width; j++)
            //    {
            //        if (i != 0 && j != 0)
            //            imageIntegrOrig[i, j] = massByteImageOrig[i, j] + imageIntegrOrig[i - 1, j] + imageIntegrOrig[i, j - 1] - imageIntegrOrig[i - 1, j - 1];
            //        else if (i == 0 && j == 0)
            //            imageIntegrOrig[0, 0] = massByteImageOrig[0, 0];
            //        else if (j == 0)
            //            imageIntegrOrig[i, j] = massByteImageOrig[i, j] + imageIntegrOrig[i - 1, j] + imageIntegrOrig[i, 0] - imageIntegrOrig[i - 1, 0];
            //        else
            //            imageIntegrOrig[i, j] = massByteImageOrig[i, j] + imageIntegrOrig[0, j] + imageIntegrOrig[i, j - 1] - imageIntegrOrig[0, j - 1];
            //    }
            //}
            #endregion
        }

        private double SrRectangleSum(int x1, int y1, int x2, int y2)
        {
            double sumLmObl = imageIntegrOrig[x2, y2] + imageIntegrOrig[x1, y1] - imageIntegrOrig[x2, y1] - imageIntegrOrig[x1, y2];
            double srObl = sumLmObl / (d * d);
            double procentOt_srObl = srObl * (procentObl / 100);
            return srObl - procentOt_srObl;
        }

        private void BradlyBinarization()
        {
            int d2 = d / 2;
            for (int i = 0; i < height; i++)
            {
                int x1 = i - d2;
                int x2 = i + d2;
                if (x1 < 0)
                    x1 = 0;
                if (x2 >= height)
                    x2 = height - 1;

                for (int j = 0; j < width; j++)
                {
                    int y1 = j - d2;
                    int y2 = j + d2;
                    if (y1 < 0)
                        y1 = 0;
                    if (y2 >= width)
                        y2 = width - 1;

                    if (massByteImageOrig[i, j] < SrRectangleSum(x1, y1, x2, y2))
                    {
                        massByteImageBinar[i, j] = black;
                    }
                    else
                    {
                        massByteImageBinar[i, j] = white;
                    }

                    if (i != 0 && j != 0)
                        imageIntegrBinar[i, j] = massByteImageBinar[i, j] + imageIntegrBinar[i - 1, j] + imageIntegrBinar[i, j - 1] - imageIntegrBinar[i - 1, j - 1];
                    else if (i == 0 && j == 0)
                        imageIntegrBinar[0, 0] = massByteImageBinar[0, 0];
                    else if (j == 0)
                        imageIntegrBinar[i, j] = massByteImageBinar[i, j] + imageIntegrBinar[i - 1, j] + imageIntegrBinar[i, 0] - imageIntegrBinar[i - 1, 0];
                    else
                        imageIntegrBinar[i, j] = massByteImageBinar[i, j] + imageIntegrBinar[0, j] + imageIntegrBinar[i, j - 1] - imageIntegrBinar[0, j - 1];
                }
            }
        }

        private double GetBrightness(double R, double G, double B)// Функция получения яркости как у Microsoft
        {
            double r = R / 255.0;
            double g = G / 255.0;
            double b = B / 255.0;

            double max, min;

            max = r; min = r;

            if (g > max) max = g;
            if (b > max) max = b;

            if (g < min) min = g;
            if (b < min) min = b;

            return (max + min) / 2;
        }
        #endregion
    }


    public class SettingThreadCam
    {




    }
}
