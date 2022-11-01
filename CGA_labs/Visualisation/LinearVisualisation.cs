using CGA_labs.Entities;
using System.Windows.Media.Imaging;

namespace CGA_labs.Visualisation
{
    public class LinearVisualisation: AbstractVisualisation
    {
        public override void DrawModel(WriteableBitmap bitmap, Model model, ModelParams parameters, Model worldModel)
        {
            foreach (var face in model.Faces)
                DrawFace(bitmap, model, face);
        }
    }
}
