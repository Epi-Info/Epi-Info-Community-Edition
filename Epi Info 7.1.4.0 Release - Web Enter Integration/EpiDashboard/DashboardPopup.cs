using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace EpiDashboard
{
    public class DashboardPopup
    {
        private Rectangle maskRectangle = new Rectangle { Fill = new SolidColorBrush(Colors.Black), Opacity = 0.0 };

        public FrameworkElement Parent
        {
            get;
            set;
        }

        public FrameworkElement Content
        {
            get;
            set;
        }

        public DashboardPopup()
        {
            Button button = new Button();
            button.Width = 100;
            button.Height = 200;
            button.Content = "I am the popup!";

            button.Click += delegate { Close(); };

            Content = button;
        }

        public void Show()
        {
            Grid grid = GetRootGrid();

            if (grid != null)
            {
                DoubleAnimation opacityAnimation = new DoubleAnimation(0.5, new Duration(TimeSpan.FromSeconds(0.15)));

                Storyboard opacityBoard = new Storyboard();
                opacityBoard.Children.Add(opacityAnimation);

                Storyboard.SetTarget(opacityAnimation, maskRectangle);
                Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("(Opacity)"));

                opacityBoard.Completed += delegate
                {
                    ScaleTransform scaleTransform = new ScaleTransform(0.0, 0.0, Content.Width / 2.0, Content.Height / 2.0);
                    Content.RenderTransform = scaleTransform;

                    grid.Children.Add(Content);
                    Grid.SetColumnSpan(Content, 100);
                    Grid.SetRowSpan(Content, 100);

                    Storyboard scaleBoard = new Storyboard();

                    DoubleAnimation scaleXAnimation = new DoubleAnimation(1.0, TimeSpan.FromSeconds(0.15));

                    scaleBoard.Children.Add(scaleXAnimation);

                    Storyboard.SetTarget(scaleXAnimation, Content);
                    Storyboard.SetTargetProperty(scaleXAnimation, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));

                    DoubleAnimation scaleYAnimation = new DoubleAnimation(1.0, TimeSpan.FromSeconds(0.15));

                    scaleBoard.Children.Add(scaleYAnimation);

                    Storyboard.SetTarget(scaleYAnimation, Content);
                    Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));

                    scaleBoard.Begin();
                };

                opacityBoard.Begin();

                grid.Children.Add(maskRectangle);
                Grid.SetColumnSpan(maskRectangle, 100);
                Grid.SetRowSpan(maskRectangle, 100);
            }
        }

        public void Close()
        {
            Grid grid = GetRootGrid();

            if (grid != null)
            {
                ScaleTransform scaleTransform = new ScaleTransform(1.0, 1.0, Content.Width / 2.0, Content.Height / 2.0);
                Content.RenderTransform = scaleTransform;

                Storyboard scaleBoard = new Storyboard();

                DoubleAnimation scaleXAnimation = new DoubleAnimation(0.0, TimeSpan.FromSeconds(0.15));

                scaleBoard.Children.Add(scaleXAnimation);

                Storyboard.SetTarget(scaleXAnimation, Content);
                Storyboard.SetTargetProperty(scaleXAnimation, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));

                DoubleAnimation scaleYAnimation = new DoubleAnimation(0.0, TimeSpan.FromSeconds(0.15));

                scaleBoard.Children.Add(scaleYAnimation);

                Storyboard.SetTarget(scaleYAnimation, Content);
                Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));

                scaleBoard.Completed += delegate
                {
                    DoubleAnimation opacityAnimation = new DoubleAnimation(0.5, 0.0, new Duration(TimeSpan.FromSeconds(0.15)));

                    Storyboard opacityBoard = new Storyboard();
                    opacityBoard.Children.Add(opacityAnimation);

                    Storyboard.SetTarget(opacityAnimation, maskRectangle);
                    Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("(Opacity)"));

                    opacityBoard.Completed += delegate
                    {
                        grid.Children.Remove(maskRectangle);
                        grid.Children.Remove(Content);
                    };

                    opacityBoard.Begin();
                };

                scaleBoard.Begin();
            }
        }

        private Grid GetRootGrid()
        {
            FrameworkElement root = Parent;

            //while (root is FrameworkElement && root.Parent != null)
            //{
            //    FrameworkElement rootElement = root as FrameworkElement;

            //    if (rootElement.Parent is FrameworkElement)
            //    {
            //        root = rootElement.Parent as FrameworkElement;
            //    }
            //}

            return root as Grid;
            //ContentControl contentControl = root as ContentControl;

            //return contentControl.Content as Grid;
        }
    }
}
