using System;
using OpenTK;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;

namespace FirstEngine
{
  public class Game : GameWindow
  {
    float[,] h = new float[128, 128];
    float angle;

    private static NativeWindowSettings native = new()
    {
      Title = "My First Render",
      APIVersion = new Version(3, 3),
      Profile = ContextProfile.Compatability // No vbo
    };

    public Game()
    : base(GameWindowSettings.Default, native)
    {
      this.Size = new Vector2i(720, 720);
      this.CenterWindow(this.Size);
    }

    protected override void OnLoad()
    {
      base.OnLoad();
      GL.ClearColor(0.1f, 0.1f, 0.15f, 1f);
      GL.Enable(EnableCap.DepthTest);

      h = Utility.LoadHeights("heightmap.png");
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
      base.OnUpdateFrame(args);
      angle += (float)args.Time * 0.5f;
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
      base.OnRenderFrame(args);
      GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

      var fb = FramebufferSize;
      float aspect = fb.X / (float)fb.Y;
      // https://opentk.net/api/OpenTK.Mathematics.Matrix4.html#OpenTK_Mathematics_Matrix4_CreatePerspectiveFieldOfView_System_Single_System_Single_System_Single_System_Single_OpenTK_Mathematics_Matrix4__
      Matrix4 proj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(60f), aspect, 0.1f, 100f);

      Matrix4 view = Matrix4.LookAt(
        new Vector3(MathF.Sin(angle) * 2.5f, 1.2f, MathF.Cos(angle) * 2.5f),
        Vector3.Zero,
        Vector3.UnitY
      );

      Matrix4 model = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(-10f));

      GL.MatrixMode(MatrixMode.Projection);
      GL.LoadMatrix(ref proj);
      

      GL.MatrixMode(MatrixMode.Modelview);
      Matrix4 mv = view * model;
      GL.LoadMatrix(ref mv);

      float span = 2.0f;
      float step = span / (127);
      float start = -span / 2f;
      float heightScale = 0.4f;
      
      // 4 vertices, 2 triangle for each grid
      for (int y = 0; y < 127; y++)
      {
        for (int x = 0; x < 127; x++)
        {
          float x0 = start + x * step;
          float x1 = start + (x+1) * step;
          float z0 = start + y * step;
          float z1 = start + (y+1) * step;

          // Heights for each corner
          float y00 = h[x,y] * heightScale;
          float y10 = h[x+1,y] * heightScale;
          float y01 = h[x,y+1] * heightScale;
          float y11 = h[x+1, y+1] * heightScale;

          GL.Begin(PrimitiveType.Triangles); // Tis GL will draw immediately for every 3 vertex

          
          float avgHeight1 = (y00 + y10 + y11) / 3f; // calculate avg hight for both triangles
          GL.Color3(avgHeight1 * 0.2f, avgHeight1 * 0.4f + 0.1f, 0.2f + avgHeight1 * 0.9f);
          GL.Vertex3(x0, y00, z0);
          GL.Vertex3(x1, y10, z0);
          GL.Vertex3(x1, y11, z1);

          float avgHeight2 = (y00 + y11 + y01) / 3f;
          GL.Color3(avgHeight2 * 0.2f, avgHeight2 * 0.4f + 0.1f, 0.2f + avgHeight2 * 0.9f);
          GL.Vertex3(x0, y00, z0);
          GL.Vertex3(x1, y11, z1);
          GL.Vertex3(x0, y01, z1);

          GL.End(); // GL stop drawing triangles
        }
      }

      SwapBuffers();
    }

    protected override void OnFramebufferResize
    (FramebufferResizeEventArgs e)
    {
      base.OnFramebufferResize(e);
      GL.Viewport(0, 0, e.Width, e.Height);
    }



    private void CheckShaderCompile(int shaderHandle, string shaderName)
    {
      GL.GetShader(shaderHandle, ShaderParameter.CompileStatus, out int success);
      if (success == 0)
      {
        string infoLog = GL.GetShaderInfoLog(shaderHandle);
        Console.WriteLine($"Error compiling {shaderName}: {infoLog}");
      }
    }
  }
}