using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using WFC.Core;
using Path = System.IO.Path;

namespace WFC.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindowViewModel ViewModel => (DataContext as MainWindowViewModel)!;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void TileSelectBtn_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: int bitIndex } btn)
            {
                var tileHolder = FindParent<ItemsControl>(btn);
                var tileHolderContent = FindParent<ContentPresenter>(tileHolder);
                var container = FindParent<ContentPresenter>(tileHolderContent);

                var itemsControl = FindParent<ItemsControl>(tileHolderContent);
                var index = itemsControl.ItemContainerGenerator.IndexFromContainer(container);

                var x = index % ViewModel.Width;
                var y = index / ViewModel.Width;
                ViewModel.WFC.Collapse(x, y, bitIndex);
                ViewModel.UpdateImageBitSetByWFC();
            }
        }
        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null)
                return null;

            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindParent<T>(parentObject);
        }

        private void RandomToEndBtn_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.WFC.CollapseRandomToEnd();
            ViewModel.UpdateImageBitSetByWFC();
        }

        private void RandomStepBtn_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.WFC.CollapseRandomNext(ViewModel.NextStepCount);
            ViewModel.UpdateImageBitSetByWFC();
        }

        private void RandomBackBtn_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.WFC.BackTrack(ViewModel.BackStepCount);
            ViewModel.UpdateImageBitSetByWFC();
        }

        private void ResetBtn_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.Reset();
        }
    }
}