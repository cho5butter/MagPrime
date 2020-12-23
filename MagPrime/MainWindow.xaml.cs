using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Interop;
using System.Collections.ObjectModel;

namespace MagPrime
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        public class Application
        {
            public ImageSource imageSource { get; set; }
            public string appName { get; set; }
        }
        public MainWindow()
        {
            InitializeComponent();

            ObservableCollection<Application> apps = new ObservableCollection<Application>();
            foreach (Process p in Process.GetProcesses())
            {
                if (p.MainWindowTitle.Length != 0)
                {
                    //try
                    //{
                        Console.WriteLine(p.MainModule.FileName);
                        System.Drawing.Icon ico = System.Drawing.Icon.ExtractAssociatedIcon(p.MainModule.FileName);
                        Bitmap bitmap = ico.ToBitmap();
                        IntPtr hBitmap = bitmap.GetHbitmap();
                        ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                        apps.Add(new Application() { imageSource = wpfBitmap, appName = p.MainWindowTitle });

                    //}
                    //catch(System.ComponentModel.Win32Exception)
                    //{
                    //    Console.WriteLine(p.MainWindowTitle);
                    //}
                }
            }
            appList.ItemsSource = apps;
        }
    }
}
