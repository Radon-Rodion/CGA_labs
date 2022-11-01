using System.Windows;

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
    }
}
