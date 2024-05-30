using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static AOI.BaseLogRecord;

namespace AOI
{
    public class Parameter
    {
        public int ID { get; set; }
        public string Name { get; set; }

    }
    
    public partial class MainWindow : System.Windows.Window
    {
        
        public MainWindow()
        {
            InitializeComponent();
        }

        #region Function
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("請問是否要關閉？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void Display_Image_MouseMove(object sender, MouseEventArgs e)
        {
            System.Windows.Point position = e.GetPosition(Display_Image);
            if (Display_Image.Source != null)
            {
                BitmapSource bitmapSource = (BitmapSource)Display_Image.Source;
                int x = (int)(position.X * bitmapSource.PixelWidth / Display_Image.ActualWidth);
                int y = (int)(position.Y * bitmapSource.PixelHeight / Display_Image.ActualHeight);
                if (x >= 0 && x < bitmapSource.PixelWidth && y >= 0 && y < bitmapSource.PixelHeight)
                {
                    CroppedBitmap crop = new CroppedBitmap(bitmapSource, new Int32Rect(x, y, 1, 1));
                    byte[] pixels = new byte[4];
                    crop.CopyPixels(pixels, 4, 0);
                    RGB.Text = $"RGB:";
                    R.Text = $"{pixels[2]},";
                    G.Text = $"{pixels[1]},";
                    B.Text = $"{pixels[0]}";
                }
            }
        }

        private BitmapImage MatToBitmapImage(Mat mat)
        {
            Bitmap bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mat);
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                return bitmapImage;
            }
        }
        #endregion

        #region Parameter and Init
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Mag.IsEnabled = false;
        }
        Algorithm Do = new Algorithm();
        #region Log
        BaseLogRecord Logger = new BaseLogRecord();
        //Logger.WriteLog("儲存參數!", LogLevel.General, richTextBoxGeneral);
        #endregion
        #region Config
        BaseConfig<Parameter> Config = new BaseConfig<Parameter>();
        //Load Config
        //List<Parameter> Parameter_info = Config.Load();
        //Console.WriteLine(Parameter_info[0].ID);
        //Save Config
        //List<Parameter> Parameter_config = new List<Parameter>()
        //{
        //    new Parameter() { ID = 1, Name = "張飛"}
        //};
        //Config.Save(Parameter_config);
        #endregion
        #endregion

        #region Main Screen
        private void Main_Btn_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as Button).Name)
            {
                case nameof(Open_Image_Path):
                    {
                        OpenFileDialog openFileDialog = new OpenFileDialog();
                        openFileDialog.Filter = "Image files|*.bmp;*.jpg;*.png;*|All files|*.*";
                        if (openFileDialog.ShowDialog() == true)
                        {
                            try
                            {
                                BitmapImage bitmapImage = new BitmapImage(new Uri(openFileDialog.FileName));
                                Display_Image.Source = bitmapImage;
                                Image_Path.Text = openFileDialog.FileName;
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Error: " + ex.Message);
                            }
                        }
                        break;
                    }
                case nameof(Pre_Annotation):
                    {
                        string imagefile = Image_Path.Text;
                        if (!string.IsNullOrEmpty(imagefile))
                        {
                            Mat src = Cv2.ImRead(imagefile, ImreadModes.Color);
                            Display_Image.Source = MatToBitmapImage(Do.BoundingBox(src, 120));
                        }
                        else
                        {
                            MessageBox.Show("請輸入影像路徑!", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        break;
                    }
            }
        }
        #endregion

        #region CheckBox event
        private void Switch_Mag_Checked(object sender, RoutedEventArgs e)
        {
            Mag.IsEnabled = true;
        }
        private void Switch_Mag_Unchecked(object sender, RoutedEventArgs e)
        {
            Mag.IsEnabled = false;
        }
        #endregion


    }
}
