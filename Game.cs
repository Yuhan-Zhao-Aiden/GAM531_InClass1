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
    private int timeLoc, rotationLoc, scaleLoc, centerLoc;
    private float time, angle = 0f, scale;
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
      // THis is from A1, if we fix the window size it will be square, currently not dynamic
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

      // I'm changing the vertex shader to calculate and output uv to fragment shader
      string vertexShaderCode = @"
        #version 330 core
        layout(location = 0) in vec3 aPosition;

        out vec2 vUV;
        uniform mat2 uRotation;
        uniform float uScale;
        uniform vec2 uCenter;

        void main()
        {
          vec2 pos = uCenter + uRotation * (aPosition.xy * uScale);
          gl_Position = vec4(pos, aPosition.z, 1.0);
          vUV = aPosition.xy + vec2(0.5);
        }
      ";

      string fragmentShaderCode = @"
      #version 330 core
      in vec2 vUV;
      out vec4 FragColor;

      uniform float uTime;

      void main()
      {
        float red = clamp(vUV.x, 0.0, 1.0);
        float green = clamp(vUV.y, 0.0, 1.0);
        float blue = 0.5 + 0.5 * sin(uTime); // this is additional from exercise

        FragColor = vec4(red, green, blue, 1.0);
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

      timeLoc = GL.GetUniformLocation(shaderProgramHandle, "uTime");
      rotationLoc = GL.GetUniformLocation(shaderProgramHandle, "uRotation");
      scaleLoc = GL.GetUniformLocation(shaderProgramHandle, "uScale");
      centerLoc = GL.GetUniformLocation(shaderProgramHandle, "uCenter");


      GL.DetachShader(shaderProgramHandle, vShaderHandle);
      GL.DetachShader(shaderProgramHandle, fShaderHandle);
      GL.DeleteShader(vShaderHandle);
      GL.DeleteShader(fShaderHandle);
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
      base.OnUpdateFrame(args);
      time += (float)args.Time;
      angle = time * 1.0f;
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
      base.OnRenderFrame(args);
      GL.Clear(ClearBufferMask.ColorBufferBit);
      GL.UseProgram(shaderProgramHandle);

      GL.Uniform1(timeLoc, time);

      float cosA = MathF.Cos(angle);
      float sinA = MathF.Sin(angle);
      // build 2d rotation matrix
      float[] rot = new float[] {
        cosA, sinA,
        -sinA, cosA
      };
      scale = 0.5f * (1.0f + 0.25f * MathF.Sin(time * 2.0f));
      // rotation scale and center
      GL.UniformMatrix2(rotationLoc, 1, false, rot);
      GL.Uniform1(scaleLoc, scale);
      GL.Uniform2(centerLoc, new Vector2i(0, 0));


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