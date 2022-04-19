namespace RepoZ.App.Win.Controls
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    public class AcrylicMenuItem : MenuItem
    {
        protected override void OnSubmenuOpened(RoutedEventArgs e)
        {
            base.OnSubmenuOpened(e);

            Dispatcher.BeginInvoke((Action)BlurSubMenu);
        }

        private void BlurSubMenu()
        {
            DependencyObject firstSubItem = ItemContainerGenerator.ContainerFromIndex(0);

            if (firstSubItem == null)
            {
                return;
            }

            var container = VisualTreeHelper.GetParent(firstSubItem) as Visual;

            if (container == null)
            {
                return;
            }

            DependencyObject parent = container;
            var borderIndex = 0;

            while (parent != null)
            {
                if (parent is Border b)
                {
                    // only put color on the first border (transparent colors will add up otherwise)
                    if (borderIndex == 0)
                    {
                        b.Background = new SolidColorBrush(Color.FromArgb(80, 0, 0, 0));
                    }
                    else
                    {
                        b.Background = Brushes.Transparent;
                    }

                    borderIndex++;
                }

                parent = VisualTreeHelper.GetParent(parent);
            }

            AcrylicHelper.EnableBlur(container);
        }
    }
}