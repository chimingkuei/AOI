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

        private void DrawROI(System.Windows.Point _downPoint, System.Windows.Rect rect)
        {
            double ratio_x = (double)target_image.Width / 708;
            double ratio_y = (double)target_image.Height / 404;
            int x = (int)(_downPoint.X * ratio_x);
            int y = (int)(_downPoint.Y * ratio_y);
            int w = (int)(rect.Width * ratio_x);
            int h = (int)(rect.Height * ratio_y);
            OpenCvSharp.Rect roi = new OpenCvSharp.Rect(x, y, w, h);
            Mat roiImage = new Mat(target_image, roi);
            Logger.WriteLog("灰階平均值:" + Do.CalculateGrayAverage(roiImage).ToString(), LogLevel.General, richTextBoxGeneral);
        }

        private void ShowROI(System.Windows.Shapes.Rectangle ROI, System.Windows.Rect rect)
        {
            ROI.Margin = new Thickness(rect.Left, rect.Top, 0, 0);
            ROI.Width = rect.Width;
            ROI.Height = rect.Height;
        }
        #endregion

        #region Parameter and Init
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Mag.IsEnabled = false;
            Red_Rect.Visibility = Visibility.Hidden;
            Blue_Rect.Visibility = Visibility.Hidden;
        }
        Algorithm Do = new Algorithm();
        BaseLogRecord Logger = new BaseLogRecord();
        private bool _started1;
        private System.Windows.Point _downPoint1;
        private System.Windows.Rect rect1;
        private bool _started1_state;
        private bool _started2;
        private System.Windows.Point _downPoint2;
        private System.Windows.Rect rect2;
        private bool _started2_state;
        private Mat target_image;
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
                                target_image = Cv2.ImRead(Image_Path.Text);
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
                            Display_Image.Source = MatToBitmapImage(Do.BoundingBox(target_image, 120));
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

        #region MouseButton event
        private void DrawROI_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Red_Rect.Visibility = Visibility.Visible;
                _started1 = true;
                _downPoint1 = e.GetPosition(Display_Image);
                //Console.WriteLine($"X座標:{_downPoint1.X}");
                //Console.WriteLine($"Y座標:{_downPoint1.Y}");
            }
            if (e.RightButton == MouseButtonState.Pressed)
            {
                Blue_Rect.Visibility = Visibility.Visible;
                _started2 = true;
                _downPoint2 = e.GetPosition(Display_Image);
                //Console.WriteLine($"X座標:{_downPoint2.X}");
                //Console.WriteLine($"Y座標:{_downPoint2.Y}");
            }
        }

        private void DrawROI_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_started1_state)
            {
                _started1 = false;
                DrawROI(_downPoint1, rect1);
                _started1_state = false;
            }
            if (_started2_state)
            {
                _started2 = false;
                DrawROI(_downPoint2, rect2);
                _started2_state = false;
            }
        }

        private void DrawROI_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (_started1)
                {
                    var point = e.GetPosition(Display_Image);
                    rect1 = new System.Windows.Rect(_downPoint1, point);
                    ShowROI(Red_Rect, rect1);
                    _started1_state = true;
                }
            }
            if (e.RightButton == MouseButtonState.Pressed)
            {
                if (_started2)
                {
                    var point = e.GetPosition(Display_Image);
                    rect2 = new System.Windows.Rect(_downPoint2, point);
                    ShowROI(Blue_Rect, rect2);
                    _started2_state = true;
                }
            }
        }
        #endregion

    }
}
