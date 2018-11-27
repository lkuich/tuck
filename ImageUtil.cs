using System.IO;
using TensorFlow;
using System.Drawing;

public static class ImageUtil
{

    // TODO: Adjust to size of your image
    const int INPUT_SIZE = 128;
    const int IMAGE_MEAN = 128;
    const float IMAGE_STD = 128;

    public static TFTensor CreateTensorFromImageFile (string file)
    {
        var b = new Bitmap(file, true);
        var bitmap = new Bitmap(b, INPUT_SIZE, INPUT_SIZE);
        
        Color[] colors = new Color[bitmap.Size.Width * bitmap.Size.Height];
        
        int z = 0;
        for (int y = bitmap.Size.Height -1; y >= 0; y--) {
            for (int x = 0; x < bitmap.Size.Width; x++) {
                colors[z] = bitmap.GetPixel(x, y);
                z++;
            }
        }

        float[] floatValues = new float[(INPUT_SIZE * INPUT_SIZE) * 3];
        for (int i = 0; i < colors.Length; i++) {
            var color = colors[i];

            floatValues[i * 3] = (color.R - IMAGE_MEAN) / IMAGE_STD;
            floatValues[i * 3 + 1] = (color.G - IMAGE_MEAN) / IMAGE_STD;
            floatValues[i * 3 + 2] = (color.B - IMAGE_MEAN) / IMAGE_STD;
        }

        TFShape shape = new TFShape(1, INPUT_SIZE, INPUT_SIZE, 3);
        return TFTensor.FromBuffer(shape, floatValues, 0, floatValues.Length);
    }
}