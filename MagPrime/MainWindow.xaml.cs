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
            public Process appProcess { get; set; }
        }
        private struct RECT
        {
            //型はlongじゃなくてintが正解！！！！！！！！！！！！！！
            //longだとおかしな値になる
            public int left;
            public int top;
            public int right;
            public int bottom;
        }
        private ObservableCollection<Application> _apps;
        public MainWindow()
        {
            InitializeComponent();

            this._apps = new ObservableCollection<Application>();
            foreach (Process p in Process.GetProcesses())
            {
                if (p.MainWindowTitle.Length != 0)
                {
                    try
                    {
                        Console.WriteLine(p.MainModule.FileName);
                        System.Drawing.Icon ico = System.Drawing.Icon.ExtractAssociatedIcon(p.MainModule.FileName);
                        Bitmap bitmap = ico.ToBitmap();
                        IntPtr hBitmap = bitmap.GetHbitmap();
                        ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                        this._apps.Add(new Application() {
                            imageSource = wpfBitmap,
                            appName = p.MainWindowTitle,
                            appProcess = p
                        });

                    }
                    catch(System.ComponentModel.Win32Exception)
                    {
                        Console.WriteLine(p.MainWindowTitle);
                    }
                }
            }
            appList.ItemsSource = _apps;
        }
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, int uFlags);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        const int SWP_NOMOVE = 0x0001;
        const int SWP_NOZORDER = 0x0004;
        void PrintText(object sender, SelectionChangedEventArgs args)
        {
            if (appList.SelectedIndex == -1) return;
            Process p = this._apps[appList.SelectedIndex].appProcess;
            p.WaitForInputIdle();
            //MoveWindow(p.MainWindowHandle, 100, 100, 300, 300, 1);
            int windowWidth = 500;
            int windowHeight = 500;
            if(GetWindowRect(p.MainWindowHandle, out RECT wRect))
            {
                windowWidth = wRect.right - wRect.left;
                windowHeight = wRect.bottom - wRect.top;
            }
            SetWindowPos(
                p.MainWindowHandle,
                0,
                (System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width - windowWidth)/2,
                (System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height - windowHeight) / 2,
                300,
                300,
                SWP_NOMOVE | SWP_NOZORDER
                );
            appList.SelectedIndex = -1;
            this.Close();
        }
    }
}
