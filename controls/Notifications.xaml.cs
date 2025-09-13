using MainDabRedo;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MainDabRedo.controls
{
    public partial class Notifications : UserControl
    {
        private MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
        private DispatcherTimer _timer;
        private Stopwatch _stopwatch;
        private TimeSpan _totalTime;
        private bool _isPaused = false;

        public int Time { get; set; } = 5;

        public Color BackgroundColor
        {
            get => ((SolidColorBrush)Notif.Background).Color;
            set => Notif.Background = new SolidColorBrush(value);
        }

        public Color ProgressColor
        {
            get => ((SolidColorBrush)Progressbar.Background).Color;
            set => Progressbar.Background = new SolidColorBrush(value);
        }

        public Color BProgressColor
        {
            get => ((SolidColorBrush)BProgressbar.Background).Color;
            set => BProgressbar.Background = new SolidColorBrush(value);
        }

        public string NotificationText
        {
            get => Title.Text;
            set => Title.Text = value;
        }

        public ImageSource NotificationImage
        {
            get => Image.Source;
            set => Image.Source = value;
        }

        public Notifications()
        {
            InitializeComponent();
            this.MouseEnter += UserControl_MouseEnter;
            this.MouseLeave += UserControl_MouseLeave;
        }

        public void StartNotification()
        {
            this.RenderTransform = new TranslateTransform();

            Storyboard showStoryboard = new Storyboard();
            DoubleAnimation slideIn = new DoubleAnimation
            {
                From = 500,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(350),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            Storyboard.SetTarget(slideIn, this);
            Storyboard.SetTargetProperty(slideIn, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));
            showStoryboard.Children.Add(slideIn);
            showStoryboard.Begin();

            _totalTime = TimeSpan.FromSeconds(Time);
            _stopwatch = new Stopwatch();
            _stopwatch.Start();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(50)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_isPaused)
                return;

            var elapsed = _stopwatch.Elapsed;
            if (elapsed >= _totalTime)
            {
                _timer.Stop();
                CloseNotification();
            }
            else
            {
                double progressPercentage = elapsed.TotalSeconds / _totalTime.TotalSeconds;
                Progressbar.Width = 300 * progressPercentage;
            }
        }

        private void CloseNotification()
        {
            this.RenderTransform = new TranslateTransform();

            Storyboard closeStoryboard = new Storyboard();
            DoubleAnimation slideOut = new DoubleAnimation
            {
                From = 0,
                To = 500,
                Duration = TimeSpan.FromMilliseconds(350),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            Storyboard.SetTarget(slideOut, this);
            Storyboard.SetTargetProperty(slideOut, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));
            closeStoryboard.Children.Add(slideOut);
            closeStoryboard.Completed += (s, e) =>
            {
                Dispatcher.InvokeAsync(() =>
                {
                    var parent = this.Parent as Panel;
                    parent?.Children.Remove(this);
                }, DispatcherPriority.ApplicationIdle);
            };
            closeStoryboard.Begin();

            AnimateOpacity(this, 1, 0, 0.35);
        }

        private void AnimateOpacity(UIElement element, double fromOpacity, double toOpacity, double durationSeconds)
        {
            var opacityAnimation = new DoubleAnimation
            {
                From = fromOpacity,
                To = toOpacity,
                Duration = new Duration(TimeSpan.FromSeconds(durationSeconds))
            };

            element.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            _isPaused = true;
            _timer.Stop();
            _stopwatch.Stop();
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            _isPaused = false;
            _stopwatch.Start();
            _timer.Start();
        }

        private void Cancel_MouseEnter(object sender, MouseEventArgs e)
        {
            AnimateOpacity(closedImg, closedImg.Opacity, 1, 0.2);
        }

        private void Cancel_MouseLeave(object sender, MouseEventArgs e)
        {
            AnimateOpacity(closedImg, closedImg.Opacity, 0, 0.2);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            CloseNotification();
        }
    }
}