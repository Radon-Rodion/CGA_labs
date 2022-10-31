using CGA_labs.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace CGA_labs.Visualisation
{
    public class LinearVisualisator: AbstractVisualisator
    {
        public override void DrawModel(WriteableBitmap bitmap, Model model, ModelParams parameters, Model worldModel)
        {
            foreach (var face in model.Faces)
                DrawFace(bitmap, model, face);
        }
    }
}
