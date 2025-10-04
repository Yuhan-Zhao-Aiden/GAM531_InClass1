using System;
using System.IO;
using StbImageSharp;

public class Utility
{
  public static float[,] LoadHeights(string path)
  {
    using var stream = File.OpenRead(path);
    var image = ImageResult.FromStream(stream, ColorComponents.Grey);

    if (image.Width != 128 || image.Height != 128)
    {
      throw new Exception("height map size mismatch");
    }

    float[,] h = new float[128, 128];

    for (int y = 0; y < 128; y++)
    {
      for (int x = 0; x < 128; x++)
      {
        byte value = image.Data[y * image.Width + x];
        h[x, y] = value / 255f; 
      }
    }
    
    return h;
  }
}
