using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace KlipViewer
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        private LowLevelKeyboardListener _listener;

        private string _clipboardTextContent;
        public string ClipboardTextContent
        {
            get => _clipboardTextContent;
            set
            {
                _clipboardTextContent = value;
                OnPropertyChanged("ClipboardTextContent");
            }
        }

        private BitmapSource _clipboardImageContent;
        public BitmapSource ClipboardImageContent
        {
            get => _clipboardImageContent;
            set
            {
                _clipboardImageContent = value;
                OnPropertyChanged("ClipboardImageContent");
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            mainWindow.Left = SystemParameters.PrimaryScreenWidth - this.Width;

            _listener = new LowLevelKeyboardListener();
            _listener.OnKeyPressed += _listener_OnKeyPressed;

            _listener.HookKeyboard();

            EvaluateClipboardData();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _listener.UnHookKeyboard();
        }
        private async void _listener_OnKeyPressed(object sender, KeyPressedArgs e)
        {
            if (e.KeyPressed.ToString().ToLower() == "scroll")
            {
                this.Activate();
                mainWindow.Visibility = (mainWindow.Visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
            }
            else if(e.KeyPressed.ToString().ToLower() == "escape")
            {
                System.Windows.Forms.Clipboard.Clear();
                ClipboardImage.Visibility = Visibility.Collapsed;
                ClipboardText.Visibility = Visibility.Collapsed;
                ClipboardTextContent = "";
            }
            
            if(mainWindow.Visibility == Visibility.Visible && (e.KeyPressed.ToString().ToLower() == "c" || e.KeyPressed.ToString().ToLower() == "snapshot" || e.KeyPressed.ToString().ToLower() == "scroll"))
            {
                // If you don't wait for the clipboard to actually write, EvaluteClipboardData can accidently capture the 'previous' state, thus await 25ms.
                await Task.Delay(25);
                EvaluateClipboardData();
                GC.Collect(); // One of the few times forcing collect seems to be a good idea*. (famous last words).
                // TODO:  Understand why GC.Collect() works here and find out why it's actually my fault that the memory footprint explodes on visibility toggles.
            }
        }

        private void EvaluateClipboardData()
        {
            if(System.Windows.Forms.Clipboard.ContainsImage())
            {
                ClipboardImage.Visibility = Visibility.Visible;
                ClipboardText.Visibility = Visibility.Collapsed;

                ShowClipboardImage();
            }
            else if(System.Windows.Forms.Clipboard.ContainsText())
            {
                ClipboardImage.Visibility = Visibility.Collapsed;
                ClipboardText.Visibility = Visibility.Visible;

                System.Windows.Forms.IDataObject clipboardTextObject = System.Windows.Forms.Clipboard.GetDataObject();
                ClipboardTextContent = (string)clipboardTextObject.GetData(System.Windows.Forms.DataFormats.Text);

                Console.WriteLine(ClipboardTextContent);
            }
        }

        private void ShowClipboardImage()
        {
            System.Windows.Forms.IDataObject clipboardData = System.Windows.Forms.Clipboard.GetDataObject();

            if (clipboardData != null)
            {
                if (clipboardData.GetDataPresent(System.Windows.Forms.DataFormats.Bitmap))
                {
                    System.Drawing.Bitmap bitmap = (System.Drawing.Bitmap)clipboardData.GetData(System.Windows.Forms.DataFormats.Bitmap);
                    IntPtr hBitmap = bitmap.GetHbitmap();
                    try
                    {
                        ClipboardImageContent = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    }
                    finally
                    {
                        DeleteObject(hBitmap);
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
