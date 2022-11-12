using CGA_labs.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CGA_labs.Logic
{
    public interface IKeydownProcessor
    {
        void Process(ModelParams modelParams, KeyEventArgs e);
    }

    public class ModelMover : IKeydownProcessor
    {
        public void Process(ModelParams modelParams, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    modelParams.TranslationZ += 1;
                    break;
                case Key.Down: 
                    modelParams.TranslationZ -= 1;
                    break;
                case Key.Left:
                    modelParams.TranslationX -= 1;
                    break;
                case Key.Right:
                    modelParams.TranslationX += 1;
                    break;
                case Key.PageUp:
                    modelParams.TranslationY += 1;
                    break;
                case Key.PageDown:
                    modelParams.TranslationY -= 1;
                    break;
            }
        }
    }

    public class ModelRotator : IKeydownProcessor
    {
        public void Process(ModelParams modelParams, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    modelParams.ModelPitch -= (float)Math.PI / 12;
                    break;
                case Key.Down:
                    modelParams.ModelPitch += (float)Math.PI / 12;
                    break;
                case Key.Left:
                    modelParams.ModelRoll += (float)Math.PI / 12;
                    break;
                case Key.Right:
                    modelParams.ModelRoll -= (float)Math.PI / 12;
                    break;
                case Key.PageUp:
                    modelParams.ModelYaw -= (float)Math.PI / 12;
                    break;
                case Key.PageDown:
                    modelParams.ModelYaw += (float)Math.PI / 12;
                    break;
            }
        }
    }

    public class CameraMover : IKeydownProcessor
    {
        public void Process(ModelParams modelParams, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    modelParams.CameraPositionZ -= 1;
                    break;
                case Key.Down:
                    modelParams.CameraPositionZ += 1;
                    break;
                case Key.Left:
                    modelParams.CameraPositionX += 1;
                    break;
                case Key.Right:
                    modelParams.CameraPositionX -= 1;
                    break;
                case Key.PageUp:
                    modelParams.CameraPositionY -= 1;
                    break;
                case Key.PageDown:
                    modelParams.CameraPositionY += 1;
                    break;
            }
        }
    }

    public class CameraRotator : IKeydownProcessor
    {
        public void Process(ModelParams modelParams, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    modelParams.CameraRoll += (float)Math.PI / 12;
                    break;
                case Key.Down:
                    modelParams.CameraRoll -= (float)Math.PI / 12;
                    break;
                case Key.Left:
                    modelParams.CameraYaw += (float)Math.PI / 12;
                    break;
                case Key.Right:
                    modelParams.CameraYaw -= (float)Math.PI / 12;
                    break;
                case Key.PageUp:
                    modelParams.CameraPitch += (float)Math.PI / 12;
                    break;
                case Key.PageDown:
                    modelParams.CameraPitch -= (float)Math.PI / 12;
                    break;
            }
        }
    }
}
