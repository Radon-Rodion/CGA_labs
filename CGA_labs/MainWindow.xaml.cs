using CGA_labs.Entities;
using CGA_labs.Logic;
using CGA_labs.Visualisation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CGA_labs
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IKeydownProcessor _keydownProcessor;
        private AbstractVisualisator _visualisator;
        private ModelParams _params;
        private Model _model;

        public MainWindow()
        {
            InitializeComponent();
            _keydownProcessor = new ModelRotator();
            _visualisator = new PBRVisualisator();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (_params is not null)
            {
                _keydownProcessor.Process(_params, e);
                var width = pictureContainer.ActualWidth;
                var height = pictureContainer.ActualHeight;

                Draw((int)width, (int)height);
            }
        }

        private void MoveModelRadio_Checked(object sender, RoutedEventArgs e)
        {
            _keydownProcessor = new ModelMover();
        }

        private void RotateModelRadio_Checked(object sender, RoutedEventArgs e)
        {
            _keydownProcessor = new ModelRotator();
        }

        private void MoveCameraRadio_Checked(object sender, RoutedEventArgs e)
        {
            _keydownProcessor = new CameraMover();
        }

        private void RotateCameraRadio_Checked(object sender, RoutedEventArgs e)
        {
            _keydownProcessor = new CameraRotator();
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            _model = IOLogic.ReadObjFile(
                IOLogic.ChooseFile());
            if (_model is not null)
            {
                var width = pictureContainer.ActualWidth;
                var height = pictureContainer.ActualHeight;
                _params = new ModelParams(width, height, CommonVisualisationLogic.FindStartCameraY(_model), CommonVisualisationLogic.FindStartCameraZ(_model));

                Draw((int)width, (int)height);
            }
        }

        private void Draw(int width, int height)
        {
            var bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            Model modelMain = _model.Clone() as Model;
            TransformationLogic.TransformFromModelToView(modelMain, _params);

            _visualisator.DrawModel(bitmap, modelMain, _params, TransformationLogic.TransformFromModelToWorld(_model.Clone() as Model, _params));
            picture.Source = bitmap;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if(_params is not null)
            {
                var width = pictureContainer.ActualWidth;
                var height = pictureContainer.ActualHeight;
                _params = new ModelParams(width, height, CommonVisualisationLogic.FindStartCameraY(_model) ,CommonVisualisationLogic.FindStartCameraZ(_model));
                Draw((int)width, (int)height);
            }
        }
    }
}
