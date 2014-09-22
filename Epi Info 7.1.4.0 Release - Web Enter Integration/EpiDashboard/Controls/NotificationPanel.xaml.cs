using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Timers;
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

namespace EpiDashboard.Controls
{
    /// <summary>
    /// Interaction logic for NotificationPanel.xaml
    /// </summary>
    public partial class NotificationPanel : UserControl
    {
        private delegate void SimpleCallback();

        public event NotificationButtonHandler ActionButtonClicked;

        private BackgroundWorker worker;
        private Queue<NotificationMessage> messageQueue;
        private Timer messageTimer;
        private object syncLock = new object();

        public NotificationPanel()
        {
            InitializeComponent();
            closeButton.Clicked += new GadgetCloseButtonHandler(closeButton_Clicked);
            messageQueue = new Queue<NotificationMessage>();
            this.IsHitTestVisible = false;
        }

        public void AddMessage(NotificationMessage message)
        {
            messageQueue.Enqueue(message);            

            //if (!message.RequiresInteraction)
            //{
                btnAction.Visibility = System.Windows.Visibility.Collapsed;
                worker = new System.ComponentModel.BackgroundWorker();                
                worker.DoWork += new System.ComponentModel.DoWorkEventHandler(worker_DoWork);
                worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(worker_WorkerCompleted);
                worker.RunWorkerAsync(message);
            //}
        }

        /// <summary>
        /// Handles the DoWorker event for the worker
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void worker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            lock (syncLock)
            {
                while (messageTimer != null) { }

                NotificationMessage message = (NotificationMessage)e.Argument;
                messageTimer = new Timer();
                if (message.RequiresInteraction)
                {
                }
                else
                {
                    messageTimer.Elapsed += new ElapsedEventHandler(TimerElapsed);
                }
                messageTimer.Interval = 1000 * message.DisplayDuration;
                messageTimer.Start();
                e.Result = message;                
            }
        }

        /// <summary>
        /// Handles the WorkerCompleted event for the worker
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void worker_WorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            NotificationMessage message = (NotificationMessage)e.Result;
            SetNotificationPanelText(message.Message);
            ShowNotificationPanel();
            if (message.RequiresInteraction)
            {
                btnAction.Visibility = System.Windows.Visibility.Visible;
                switch (message.ButtonType)
                {
                    case NotificationButtonType.OK:
                        btnAction.Content = "OK";
                        break;
                    case NotificationButtonType.Close:
                        btnAction.Content = "Close";
                        break;
                    case NotificationButtonType.Cancel:
                        btnAction.Content = "Cancel";
                        break;
                    case NotificationButtonType.Run:
                        btnAction.Content = "Run";
                        break;
                    case NotificationButtonType.Save:
                        btnAction.Content = "Save";
                        break;
                }
            }
            else
            {
                btnAction.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void EndDisplayMessage()
        {
            if (messageQueue.Count > 0)
            {
                messageQueue.Dequeue();
            }

            if (messageTimer != null)
            {
                messageTimer.Stop();
                messageTimer.Elapsed -= new ElapsedEventHandler(TimerElapsed);
                messageTimer = null;
            }

            if (messageQueue.Count == 0)
            {
                this.Dispatcher.BeginInvoke(new SimpleCallback(HideNotificationPanel));
            }
        }

        private void TimerElapsed(object source, ElapsedEventArgs e)
        {
            EndDisplayMessage();
        }

        private void btnAction_Click(object sender, RoutedEventArgs e)
        {
            if (ActionButtonClicked != null)
            {
                ActionButtonClicked();
            }

            EndDisplayMessage();
        }

        private void closeButton_Clicked()
        {
            EndDisplayMessage();
        }

        private void HideNotificationPanel()
        {
            Storyboard storyboard = new Storyboard();
            TimeSpan duration = new TimeSpan(0, 0, 0, 0, 550);

            DoubleAnimation animation = new DoubleAnimation();

            animation.From = 1.0;
            animation.To = 0.0;
            animation.Duration = new Duration(duration);

            Storyboard.SetTargetName(animation, panelNotification.Name);
            Storyboard.SetTargetProperty(animation, new PropertyPath(Control.OpacityProperty));

            storyboard.Children.Add(animation);
            storyboard.Begin(this);

            this.IsHitTestVisible = false;
        }

        private void ShowNotificationPanel()
        {
            this.IsHitTestVisible = true;
            this.Visibility = System.Windows.Visibility.Visible;

            if (panelNotification.Opacity < 1.0)
            {
                Storyboard storyboard = new Storyboard();
                TimeSpan duration = new TimeSpan(0, 0, 0, 0, 150);

                DoubleAnimation animation = new DoubleAnimation();

                animation.From = 0.0;
                animation.To = 1.0;
                animation.Duration = new Duration(duration);

                Storyboard.SetTargetName(animation, panelNotification.Name);
                Storyboard.SetTargetProperty(animation, new PropertyPath(Control.OpacityProperty));

                storyboard.Children.Add(animation);

                storyboard.Begin(this);
            }
        }

        private void SetNotificationPanelText(string message)
        {
            tblockNotificationText.Text = message;
        }
    }

    public struct NotificationMessage
    {
        public string Message;
        public bool ShowActionButton;
        public NotificationButtonType ButtonType;
        public bool RequiresInteraction;
        public int DisplayDuration;

        public NotificationMessage(string message, bool showActionButton, NotificationButtonType buttonType, bool requiresInteraction, int displayDuration)
        {
            Message = message;
            ShowActionButton = showActionButton;
            ButtonType = buttonType;
            RequiresInteraction = requiresInteraction;
            if (displayDuration <= 20 && displayDuration >= 5)
            {
                DisplayDuration = displayDuration;
            }
            else if (displayDuration < 5)
            {
                DisplayDuration = 5;
            }
            else
            {
                DisplayDuration = 20;
            }
        }
    }

    public enum NotificationButtonType
    {
        None = 0,
        OK = 1,
        Cancel = 2,
        Run = 3,
        Save = 4,
        Close = 5
    }
}
