using Models;
using System.Windows;

namespace ListViewDragDropManagerDemo.Forms
{
    /// <summary>
    /// Логика взаимодействия для ControlInfoWindow.xaml
    /// </summary>
    public partial class ControlInfoWindow : Window
    {
        public ControlInfo ControlInfo { get; set; }

        public ControlInfoWindow()
        {
            InitializeComponent();
            this.Loaded += ControlInfoWindow_Loaded;
        }

        private void ControlInfoWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DataGr.DataContext = this.DataContext;
        }
    }
}
