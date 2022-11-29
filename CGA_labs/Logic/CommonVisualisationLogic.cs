using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CGA_labs.Entities;

namespace CGA_labs.Logic
{
    public static class CommonVisualisationLogic
    {
        public static void ShowErrorMessage(string errorMessage)
        {
            string messageBoxText = $"Ошибка! {errorMessage}";
            string caption = "Error";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Error;

            MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
        }

        public static float FindStartCameraZ(Model model)
        {
            float maxModelZ = model.Points.MaxBy(p => p.Z).Z;
            return maxModelZ * 3;
        }

        public static float FindStartCameraY(Model model)
        {
            float maxModelY = model.Points.MaxBy(p => p.Y).Y;
            return maxModelY/2;
        }
    }
}
