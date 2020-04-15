using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Xml.Linq;

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
        private int areaD = 324;
        private double procentObl = 15;
        private byte white = 1;
        private byte black = 0;
        private StatusBinar statusMassiv = StatusBinar.emptyOutputArray;
        private static List<UserProfil> settingList = new List<UserProfil>();
        private static object lockSetting = new object();
        //public static List<BinarBradly> imageBinariz = new List<BinarBradly>();
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
            statusMassiv = StatusBinar.noImage;
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
        /// Статус состояния готовности массивов для выгрузки.
        /// </summary>
        public StatusBinar GetStatus
        {
            get => statusMassiv;
        }

        /// <summary>
        /// Получить лист настроек (профилей).
        /// </summary>
        public static List<UserProfil> GetSettingList
        {
            get => settingList;
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
        /// </summary>
        public int[,] GetImageIntegrBinar
        {
            get => imageIntegrBinar;
        }

        /// <summary>
        /// Получить значение исследуемой области вокруг пикселя.
        /// </summary>
        public int GetArea
        {
            get => width / areaD;
        }

        /// <summary>
        /// Получить порог яркости.
        /// </summary>
        public double GetBright
        {
            get => procentObl;
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
            statusMassiv = StatusBinar.emptyOutputArray;
        }

        /// <summary>
        /// Установить D - размер области для вычисления средней яркости вокруг пикселя. (width / d)
        /// </summary>
        public void SetOblastD(int value)
        {
            if (value > 0 && value <= width)
                areaD = width / value;
            else
                throw new ArgumentOutOfRangeException("Argument is not correct.");
        }

        /// <summary>
        /// Порог для сравнения яркости пикселя со средней яркостью по области. (%)
        /// </summary>
        public void SetProcent(double value)
        {
            if (value <= 100 && value >= -100)
                procentObl = value;
            else
                throw new ArgumentOutOfRangeException("Argument is not correct.");
        }
        #endregion

        #region Start функции
        /// <summary>
        /// Запуск обработки фото без создания(Bitmap) обработанного фото.
        /// Массив байт будет состоять из 0 и 1
        /// Останется бинаризированный и интегрированный из бинаризированного массивы.
        /// </summary>
        public void StartBradlyBinar_0and1()
        {
            if (imageOrig == null) { throw new ArgumentNullException("Image not found."); }
            statusMassiv = StatusBinar.inProcess;
            white = 1;
            black = 0;
            InitMassiv();
            BitmapToByteArray();
            CreateIntegralImage();
            BradlyBinarization();
            imageIntegrOrig = null;
            massByteImageOrig = null;
            statusMassiv = StatusBinar.completed;
        }

        /// <summary>
        /// Запуск обработки фото и создать фото(Bitmap).
        /// Массив байт будет состоять из 0х00 и 0xFF (0 и 255)
        /// Останется бинаризированный и интегрированный из бинаризированного массивы.
        /// </summary>
        public void StartBradlyBinar()
        {
            if (imageOrig == null) { throw new ArgumentNullException("Image not found."); }
            statusMassiv = StatusBinar.inProcess;
            white = 0xFF;
            black = 0x00;
            InitMassiv();
            BitmapToByteArray();
            CreateIntegralImage();
            BradlyBinarization();
            ByteArrayToBitmap();
            imageIntegrOrig = null;
            massByteImageOrig = null;
            statusMassiv = StatusBinar.completed;
        }

        /// <summary>
        /// Запуск обработки фото и создать фото(Bitmap).
        /// и очистка всех массивов для освобождения памяти.
        /// Функция для того чтобы осталась только фотка. Оригинал и обработанная.
        /// </summary>
        public void StartBinarLeaveOnlyBitmap()
        {
            if (imageOrig == null) { throw new ArgumentNullException("Image not found."); }
            statusMassiv = StatusBinar.inProcess;
            white = 0xFF;
            black = 0x00;
            InitMassiv();
            BitmapToByteArray();
            CreateIntegralImage();
            BradlyBinarization(false);
            ByteArrayToBitmap();
            imageIntegrOrig = null;
            massByteImageOrig = null;
            massByteImageBinar = null;
            imageIntegrBinar = null;
            statusMassiv = StatusBinar.completed;
        }
        #endregion

        #region Работа с настройками
        /// <summary>
        /// Загрузит из XML все профили какие есть и создаст объекты UserProfil для каждого профиля в листе List<UserProfil> settingList.
        /// </summary>
        public static bool LoadProfils()
        {
            try
            {
                XDocument profils_opions = XDocument.Load(Environment.CurrentDirectory + @"\Camera Settings\SettingProfils.xml");
                XElement profil = profils_opions.Element("Profils");

                foreach (XElement xe in profil.Elements("profil").ToList())
                {
                    BinarBradly.AddSetting((string)xe.Element("Name"), (int)xe.Element("Area"), (double)xe.Element("Bright"));
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Добавить настройку в лист настроек.
        /// </summary>
        /// <returns>True - установлено, False - не установлено (возможно уже существует такая настройка)</returns>
        public static bool AddSetting(string name, int area, double bright)
        {
            lock (lockSetting)
            {
                UserProfil setting = settingList.FirstOrDefault(x => x.Name == name);
                if (setting == null)
                    settingList.Add(new UserProfil { Name = name, Area = area, ThresholdBright = bright });
                else
                    return false;
                return true;
            }
        }

        /// <summary>
        /// Изменить настройку в листе настроек.
        /// </summary>
        /// <returns>True - установлено новое значение, False - не установлено новое знач.</returns>
        public static bool EditSetting(string name, int area, double bright)
        {
            lock (lockSetting)
            {
                UserProfil setting = settingList.FirstOrDefault(x => x.Name == name);
                if (setting != null)
                {
                    setting.Area = area;
                    setting.ThresholdBright = bright;
                }
                else
                    return false;
                return true;
            }
        }

        /// <summary>
        /// Удалить настройку из листа настроек.
        /// </summary>
        /// <returns>True - удалено, False - не удаено (возможно нет такой настройки)</returns>
        public static bool DelSetting(string name)
        {
            lock (lockSetting)
            {
                return settingList.Remove(settingList.FirstOrDefault(x => x.Name == name));
            }
        }

        /// <summary>
        /// Загрузить настройку из листа настроек в экземпляр класса.
        /// </summary>
        /// <returns>True - найдено и загружено, False - не найдена настройка</returns>
        public bool LoadSetting(string name)
        {
            lock (lockSetting)
            {
                UserProfil setting = settingList.FirstOrDefault(x => x.Name == name);
                if (setting != null)
                {
                    SetOblastD(setting.Area);
                    SetProcent(setting.ThresholdBright);
                }
                else
                    return false;
                return true;
            }
        }
        #endregion

        /// <summary>
        /// Получить bitmap из 0 и 1.
        /// </summary>
        public static Bitmap ToBitmap(byte[,] array0and1, int width, int height)
        {
            Bitmap imageBinar = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            BitmapData bmpData = imageBinar.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            IntPtr ptr;
            IntPtr ptrConst = bmpData.Scan0;
            int StrideWidth = bmpData.Stride;

            for (int i = 0; i < height; i++)
            {
                ptr = ptrConst + i * StrideWidth;
                for (int j = 0; j < width; j++)
                {
                    if (array0and1[j, i] == 1)
                    {
                        array0and1[j, i] = 0xFF;
                    }
                    Marshal.WriteByte(ptr, array0and1[j, i]);
                    ptr += 0x01;
                }
            }
            imageBinar.UnlockBits(bmpData);
            return imageBinar;
        }

        /// <summary>
        /// Получить bitmap из 0 и 1. С помощью SetPixel. (долгая функция)
        /// </summary>
        public static Bitmap ToBitmap_SetPixel(byte[,] array0and1, int width, int height)
        {
            Bitmap imageBinar = new Bitmap(width, height);
            
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (array0and1[i, j] == 0)
                        imageBinar.SetPixel(i, j, Color.Black);

                    if (array0and1[i, j] == 1)
                        imageBinar.SetPixel(i, j, Color.White);

                    if (array0and1[i, j] != 0 && array0and1[i, j] != 1)
                        imageBinar.SetPixel(i, j, Color.Red);
                }
            }
            return imageBinar;
        }

        #region private функции
        private void InitMassiv()
        {
            imageIntegrOrig = new double[width, height];
            imageIntegrBinar = new int[width, height];
            massByteImageOrig = new double[width, height];
            massByteImageBinar = new byte[width, height];
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
                    Marshal.WriteByte(ptr, massByteImageBinar[j, i]);
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
                                massByteImageOrig[j, i] = Marshal.ReadByte(ptr);
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

                                massByteImageOrig[j, i] = 0.2126 * R + 0.7152 * G + 0.0722 * B;
                            }
                        }
                    }
                    break;
                default:
                    {
                        throw new FormatException("PixelFormat image is not correct.");// TypeAccessException или KeyNotFoundException или FormatException
                    }
            }
            imageOrig.UnlockBits(bmpData);
        }

        private void CreateIntegralImage()
        {
            imageIntegrOrig[0, 0] = massByteImageOrig[0, 0];//Самая быстрая реализация

            for (int i = 1; i < width; i++)
            {
                imageIntegrOrig[i, 0] = imageIntegrOrig[i - 1, 0] + massByteImageOrig[i, 0];
            }

            for (int i = 1; i < height; i++)
            {
                imageIntegrOrig[0, i] = imageIntegrOrig[0, i - 1] + massByteImageOrig[0, i];
            }

            for (int i = 1; i < width; i++)
            {
                //imageIntegrOrig[i, 0] = imageIntegrOrig[i - 1, 0] + massByteImageOrig[i, 0];//если перенести эту строчку сюда, то обработа увеличится на 0,025s примерно
                for (int j = 1; j < height; j++)
                {
                    //imageIntegrOrig[0, j] = imageIntegrOrig[0, j - 1] + massByteImageOrig[0, j];// если перенести И эту строчку сюда, то обработа увеличится в общем на 0,04s примерно
                    imageIntegrOrig[i, j] = massByteImageOrig[i, j] + imageIntegrOrig[i - 1, j] + imageIntegrOrig[i, j - 1] - imageIntegrOrig[i - 1, j - 1];
                }
            }

            #region еще варианты реализации, но они медленнее немного

            //imageIntegrOrig[0, 0] = massByteImageOrig[0, 0];//Самая быстрая реализация

            //for (int i = 1; i < width; i++)
            //{
            //    imageIntegrOrig[i, 0] = imageIntegrOrig[i - 1, 0] + massByteImageOrig[i, 0];
            //}

            //for (int i = 1; i < height; i++)
            //{
            //    imageIntegrOrig[0, i] = imageIntegrOrig[0, i - 1] + massByteImageOrig[0, i];
            //}

            //for (int i = 1; i < width; i++)
            //{
            //    //imageIntegrOrig[i, 0] = imageIntegrOrig[i - 1, 0] + massByteImageOrig[i, 0];//если перенести эту строчку сюда, то обработа увеличится на 0,025s примерно
            //    for (int j = 1; j < height; j++)
            //    {
            //        //imageIntegrOrig[0, j] = imageIntegrOrig[0, j - 1] + massByteImageOrig[0, j];// если перенести И эту строчку сюда, то обработа увеличится в общем на 0,04s примерно
            //        imageIntegrOrig[i, j] = massByteImageOrig[i, j] + imageIntegrOrig[i - 1, j] + imageIntegrOrig[i, j - 1] - imageIntegrOrig[i - 1, j - 1];
            //    }
            //}

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

            //for (int i = 1; i < width; i++)//быстрая на равне даже с самой быстрой почти.. но не стабильная.  одна и таже фотка может обрабатываться то 0,289s то 0,378s
            //{
            //    for (int j = 1; j < height; j++)
            //    {
            //        if (i != 0 && j != 0)
            //            imageIntegrOrig[i, j] = massByteImageOrig[i, j] + imageIntegrOrig[i - 1, j] + imageIntegrOrig[i, j - 1] - imageIntegrOrig[i - 1, j - 1];
            //        else if (j == 0 && i != 0)
            //            imageIntegrOrig[i, j] = massByteImageOrig[i, j] + imageIntegrOrig[i - 1, j] + imageIntegrOrig[i, 0] - imageIntegrOrig[i - 1, 0];
            //        else if (i == 0 && j != 0)
            //            imageIntegrOrig[i, j] = massByteImageOrig[i, j] + imageIntegrOrig[0, j] + imageIntegrOrig[i, j - 1] - imageIntegrOrig[0, j - 1];
            //        else
            //            imageIntegrOrig[0, 0] = massByteImageOrig[0, 0];
            //    }
            //}
            #endregion
        }

        private double SrRectangleSum(int x1, int y1, int x2, int y2)
        {
            double sumLmObl = imageIntegrOrig[x2, y2] + imageIntegrOrig[x1, y1] - imageIntegrOrig[x2, y1] - imageIntegrOrig[x1, y2];
            double srObl = sumLmObl / (areaD * areaD);
            double procentOt_srObl = srObl * (procentObl / 100);
            return srObl - procentOt_srObl;
        }

        /// <summary>
        /// Делает бинаризированное изображение из интегрированного и сразу создает
        /// итегрированное из бинаризированого.
        /// createInnegr = true - создать интегрированое из бинар
        /// createInnegr = false - не создавать интегрированое из бинар
        /// </summary>
        private void BradlyBinarization(bool createInnegr = true)
        {
            int d2 = areaD / 2;
            if (createInnegr)
            {
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

                        if (i != 0 && j != 0)
                            imageIntegrBinar[i, j] = massByteImageBinar[i, j] + imageIntegrBinar[i - 1, j] + imageIntegrBinar[i, j - 1] - imageIntegrBinar[i - 1, j - 1];
                        else if (j == 0 && i != 0)
                            imageIntegrBinar[i, j] = massByteImageBinar[i, j] + imageIntegrBinar[i - 1, j] + imageIntegrBinar[i, 0] - imageIntegrBinar[i - 1, 0];
                        else if (i == 0 && j != 0)
                            imageIntegrBinar[i, j] = massByteImageBinar[i, j] + imageIntegrBinar[0, j] + imageIntegrBinar[i, j - 1] - imageIntegrBinar[0, j - 1];
                        else
                            imageIntegrBinar[0, 0] = massByteImageBinar[0, 0];
                    }
                }
            }
            else
            {
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

    /// <summary>
    /// Статус состояния готовности массивов для выгрузки. (GetImageBinariz, GetMassByteImageBinar, GetImageIntegrBinar)
    /// </summary>
    [Flags]
    public enum StatusBinar
    {
        /// <summary>
        /// Пустой экземпляр класса. Чтобы запустить бинаризацию загрузите изображение.
        /// </summary>
        noImage = 0,
        /// <summary>
        /// Изображение в классе есть, можно запустить процесс бинаризации.
        /// </summary>
        emptyOutputArray = 1,
        /// <summary>
        /// Запущен процесс бинаризации изображения.
        /// </summary>
        inProcess = 2,
        /// <summary>
        /// Процесс бинаризации завершен. Массивы готовы для выгрузки.
        /// </summary>
        completed = 3
    };

    /// <summary>
    /// Для хранения настроек.
    /// </summary>
    public class UserProfil
    {
        public string Name { get; set; }
        public int Area { get; set; }
        public double ThresholdBright { get; set; }

        /// <summary>
        /// Сохранит в XML все профили какие есть в входном листе.
        /// </summary>
        public static bool SaveProfils(List<UserProfil> list)
        {
            try
            {
                string path = Environment.CurrentDirectory + @"\Camera Settings\";
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }

                XDocument xdoc = new XDocument();
                XElement profils_opions = new XElement("Profils");

                foreach (UserProfil profil in list)
                {
                    profils_opions.Add(new XElement($"profil",
                                           new XAttribute("ID", $"{profil.Name}"),
                                           new XElement("Name", $"{profil.Name}"),
                                           new XElement("Area", $"{profil.Area}"),
                                           new XElement("Bright", $"{profil.ThresholdBright}")));
                }

                xdoc.Add(profils_opions);
                xdoc.Save(Environment.CurrentDirectory + @"\Camera Settings\SettingProfils.xml");
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Параметр для сортировки профилей настроек по имени.
        /// Пример использования: BinarBradly.settingList.Sort(UserProfil.CompareProfils);
        /// </summary>
        public static int CompareProfils(UserProfil prof1, UserProfil prof2)
        {
            return prof1.Name.CompareTo(prof2.Name);
        }
    }
}
