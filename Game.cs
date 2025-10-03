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
    private int vbo;
    private int vao;
    private int ebo;
    private int _uLeftXLoc;
    private int _uWidthLoc;
    private int shaderProgramHandle;
    private static NativeWindowSettings native = new()
    {
      Title = "My First Render",
      APIVersion = new Version(3, 3),
      Profile = ContextProfile.Core
    };

    public Game()
    : base(GameWindowSettings.Default, native) // base constructor
    {
      this.Size = new Vector2i(720, 720);
      this.CenterWindow(this.Size);
    }

    protected override void OnFramebufferResize
    (FramebufferResizeEventArgs e)
    {
      base.OnFramebufferResize(e);
      GL.Viewport(0, 0, e.Width, e.Height);
    }

    protected override void OnLoad()
    {
      base.OnLoad();
      GL.ClearColor(new Color4(0.8f, 0.6f, 0.2f, 1f));

      // GL viewport size
      var fbSize = FramebufferSize;
      GL.Viewport(0, 0, fbSize.X, fbSize.Y);

      // vertex positions (4 vertex)
      float[] vertices = new float[]
      {
        -0.5f, 0.5f, 0.0f, //0
        0.5f, 0.5f, 0.0f, //1
        -0.5f, -0.5f, 0.0f, //2
        0.5f, -0.5f, 0.0f, //3
      };

      uint[] indices = {
        0, 1, 2, // First triangle
        1, 2, 3 // Second triangle
      };

      // VBO
      vbo = GL.GenBuffer();
      GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
      GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

      // VAO
      vao = GL.GenVertexArray();
      GL.BindVertexArray(vao);

      // EBO - for 2 triangles
      ebo = GL.GenBuffer();
      GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
      GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

      GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
      GL.EnableVertexAttribArray(0);

      // unbind
      GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
      GL.BindVertexArray(0);

      string vertexShaderCode = @"
        #version 330 core
        layout(location = 0) in vec3 aPosition;

        void main()
        {
          gl_Position = vec4(aPosition, 1.0);
        }
      ";

      string fragmentShaderCode = @"
      #version 330 core
      out vec4 FragColor;

      uniform float uLeftX;   // left X of the quad (in pixels)
      uniform float uWidth;   // width of the quad (in pixels)

      void main()
      {
        // gl_FragCoord.x is in window pixels (origin bottom-left)
        float t = clamp((gl_FragCoord.x - uLeftX) / uWidth, 0.0, 1.0);
        FragColor = vec4(0.0, 0.0, t, 1.0);
      }
      ";

      int vShaderHandle = GL.CreateShader(ShaderType.VertexShader);
      GL.ShaderSource(vShaderHandle, vertexShaderCode);
      GL.CompileShader(vShaderHandle);
      CheckShaderCompile(vShaderHandle, "Vertex Shader");

      int fShaderHandle = GL.CreateShader(ShaderType.FragmentShader);
      GL.ShaderSource(fShaderHandle, fragmentShaderCode);
      GL.CompileShader(fShaderHandle);
      CheckShaderCompile(fShaderHandle, "Fragment Shader");

      shaderProgramHandle = GL.CreateProgram();
      GL.AttachShader(shaderProgramHandle, vShaderHandle);
      GL.AttachShader(shaderProgramHandle, fShaderHandle);
      GL.LinkProgram(shaderProgramHandle);

      _uLeftXLoc  = GL.GetUniformLocation(shaderProgramHandle, "uLeftX");
      _uWidthLoc  = GL.GetUniformLocation(shaderProgramHandle, "uWidth");


      GL.DetachShader(shaderProgramHandle, vShaderHandle);
      GL.DetachShader(shaderProgramHandle, fShaderHandle);
      GL.DeleteShader(vShaderHandle);
      GL.DeleteShader(fShaderHandle);
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
      base.OnUpdateFrame(args);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
      base.OnRenderFrame(args);
      GL.Clear(ClearBufferMask.ColorBufferBit);
      GL.UseProgram(shaderProgramHandle);

      var fb = FramebufferSize;
      float W = fb.X; 

      float leftX = 0.25f * W;
      float width = 0.50f * W;

      GL.Uniform1(_uLeftXLoc, leftX);
      GL.Uniform1(_uWidthLoc, width);
      GL.BindVertexArray(vao);
      // Draw element using ebo
      GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

      SwapBuffers();
    }

    protected override void OnUnload()
    {
      // Unbind and delete buffers and shader program
      GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
      GL.DeleteBuffer(vbo);

      GL.BindVertexArray(0);
      GL.DeleteVertexArray(vao);

      GL.DeleteBuffer(ebo);

      GL.UseProgram(0);
      GL.DeleteProgram(shaderProgramHandle);

      base.OnUnload();
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