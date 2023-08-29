using System;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Input;
using Tasks.Show.Models;

namespace Tasks.Show
{
    /// <summary>
    /// Interaction logic for DialogBubble.xaml
    /// </summary>
    public partial class DialogBubble : Window
    {
        private System.Media.SoundPlayer _ding = null;
        private DispatcherTimer _tmrClose = null;
        public string _message { get; set; }
        public string _title { get; set; }
        public int _timer { get; set; }
        public Task _task { get; set; }

        public event Action<string> OnFolderRequest = (name) => { };

        public DialogBubble(string message, Task task, string title = "", int timer = 0)
        {
            InitializeComponent();
            _message = message;
            _title = title;
            _timer = timer;
            _task = task;
            _ding = new System.Media.SoundPlayer(Tasks.Show.Properties.Resources.ding);

            this.Loaded += (s, e) => 
            { 
                this.Title = string.IsNullOrEmpty(_title) ? $"Due on " + _task.Due.Value.ToString("dddd") + $" ({_task.FolderName})" : _title;
                if (_ding != null) { _ding.Play(); }
                if (App.Root.WindowHost.IsClosing) { Close(); }
            };
        }

        private void NonRectangularWindow_Loaded(object sender, RoutedEventArgs e)
        {
            tbMessage.Text = _message;
            //tbTitle.Text = string.IsNullOrEmpty(_title) ? $"Due on " + _task.Due.Value.ToString("dddd") + $" ({_task.FolderName})" : _title; //"dddd M/d/yy"
            btnTitle.Content = string.IsNullOrEmpty(_title) ? $"Due on " + _task.Due.Value.ToString("dddd") + $" ({_task.FolderName})" : _title; //"dddd M/d/yy"

            if (_timer > 0)
            {
                _tmrClose = new System.Windows.Threading.DispatcherTimer();
                _tmrClose.Interval = TimeSpan.FromSeconds(_timer);
                _tmrClose.Tick += tmrClose_Tick;
                _tmrClose.Start();
            }

        }

        /// <summary>
        /// If the user drags the dialog then assume they want to read it.
        /// </summary>
        private void NonRectangularWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _tmrClose?.Stop();
            DragMove();
        }

        /// <summary>
        /// If the user hovers over the dialog then assume they want to read it.
        /// This could become annoying to the user, so I've disabled it.
        /// </summary>
        private void NonRectangularWindow_MouseEnter(object sender, MouseEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Event disabled for convenience.");
            //_tmrClose?.Stop();
        }

        private void tmrClose_Tick(object sender, EventArgs e)
        {
            _tmrClose?.Stop();
            Close();
        }

        private void CloseButtonRectangle_Click(object sender, RoutedEventArgs e)
        {
            tbMessage.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { tbMessage.Text = "Closing..."; }));
            _tmrClose?.Stop();
            Close();
        }

        /// <summary>
        /// Go to the folder in the main task view.
        /// </summary>
        private void NonRectangularWindowButton_Click(object sender, RoutedEventArgs e)
        {
            OnFolderRequest?.Invoke($"{_task.FolderName}");
        }
    }
}
