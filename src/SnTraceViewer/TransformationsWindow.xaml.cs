using SnTraceViewer.Transformations;
using SnTraceViewer.Transformations.Builtin;
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
using System.Windows.Shapes;

namespace SnTraceViewer
{
    /// <summary>
    /// Interaction logic for TransformationsWindow.xaml
    /// </summary>
    public partial class TransformationsWindow : Window
    {
        public Transformation SelectedTransformation { get; private set; }

        public TransformationsWindow()
        {
            InitializeComponent();

            CreateButtons();
        }

        private void CreateButtons()
        {
            var transformationTypes = new[] { typeof(SaveContentTimes), typeof(TestMethodTimes) };

            foreach(var transformationType in transformationTypes)
            {
                CreateButton(transformationType);
            }
        }
        private void CreateButton(Type transformationType)
        {
            var transformation = (Transformation)Activator.CreateInstance(transformationType);

            var button = new Button
            {
                Margin = new Thickness(0, 0, 0, 4),
                Tag = transformationType,
                VerticalAlignment = VerticalAlignment.Top,
                Height = 30,
                Content = transformation.Name,
                FontSize = 14,
                FontWeight = FontWeights.Bold
            };
            button.Click += TransfotmButton_Click;

            ButtonStack.Children.Add(button);
        }

        private void TransfotmButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var transformationType = (Type)button.Tag;
            SelectedTransformation = (Transformation)Activator.CreateInstance(transformationType);
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedTransformation = null;
            Close();
        }
    }
}
