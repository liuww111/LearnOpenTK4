using System;
using System.Diagnostics;
using System.Collections.Generic;
using OpenTK;
//using OpenTK.Graphics.OpenGL; 使用高版本
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using LearnOpenTK.Common;
using FreeTypeSharp;
using FreeTypeSharp.Native;
using System.IO;

namespace LearnOpenTK
{
    /// <summary>
    /// 增加背景纹理绘制和文字混合
    /// </summary>
    public class Window : GameWindow
    {
        private readonly uint[] _indices =
        {
            0, 1, 3,
            1, 2, 3
        };

        private int _elementBufferObject;

        private int _vertexBufferObject;

        private int _vertexArrayObject;

        private Shader _shader;

        private Texture _texture1;

        private Texture _texture2;

        private System.Threading.Timer _computeTimer;

        private static decimal minVal = 0.0m;

        private static decimal change = 0.1m;

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            var Version = GL.GetString(StringName.Version);
            var Vendor = GL.GetString(StringName.Vendor);
            var Renderer=GL.GetString(StringName.Renderer);
            var ShadingLanguageVersion = GL.GetString(StringName.ShadingLanguageVersion);
            Debug.WriteLine($"Version:{Version} Vendor:{Vendor} Renderer:{Renderer} Renderer:{ShadingLanguageVersion}");
            base.VSync = VSyncMode.On;//同步的模式，此处开启垂直同步

            //https://learnopengl-cn.github.io/04%20Advanced%20OpenGL/04%20Face%20culling/
            //启用面剔除，减少看不到的面渲染
            //GL.Enable(EnableCap.CullFace);
            //剔除背向面
            //GL.CullFace(CullFaceMode.Back);
            //GL.Enable(EnableCap.CullFace);
        }
        protected override void OnLoad()
        {
            base.OnLoad();
            //设置清空屏幕所用的颜色
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            //VSync = VSyncMode.Off;

            MixChangeInit();
        }

        /// <summary>
        /// 每帧运行该函数
        /// </summary>
        /// <param name="e"></param>
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            //FPS(e);

            var key = base.KeyboardState;

            if (key.IsKeyDown(Keys.Escape))
            {
                Close();
            }
        }
         
        /// <summary>
        /// 每帧渲染
        /// </summary>
        /// <param name="e"></param>
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            //开始用设定的颜色来清空屏幕
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.BindVertexArray(_vertexArrayObject);

            Debug.WriteLine($"mixVal:{minVal % 1} ---- {(int)(minVal % 4)}");
            _shader.SetFloat("mixVal", Convert.ToSingle(minVal % 1));
            //_shader.SetFloat("mixVal", 0.5f);
            _texture2 = Texture.LoadFromFile($"Resources/Img/bag{(int)(minVal % 2)}.png");

            _texture1.Use(TextureUnit.Texture0);
            _texture2.Use(TextureUnit.Texture1);
            _shader.Use();

            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);

            base.SwapBuffers();//交换缓冲，建议最后
        }

        protected override void OnUnload() {
            base.OnUnload();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Size.X, Size.Y);
        }

        private void MixChangeInit() {
            //float xpos = 0;
            //float ypos = 0;
            float w = Size.X;
            float h = Size.Y;

            float[] vertices = {
                w-144,  h-44,  1.0f, 1.0f, // top right
                w-144, 0.0f, 1.0f, 0.0f, // bottom right
                0.0f, 0.0f, 0.0f, 0.0f, // bottom left
                0.0f,  h-44, 0.0f, 1.0f  // top left
            };

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            _shader = new Shader("Shaders/mixShader.vert", "Shaders/mixShader.frag");

            // 指定采样器的取的纹理单元
            _shader.SetInt("texture0", 0);
            _shader.SetInt("texture1", 1);

            _shader.SetMatrix4("projection", Shader.GLMOrthographic(0.0f, Size.X, 0.0f, Size.Y));//投影的场景大小

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);

            _texture1 = Texture.LoadFromFile("Resources/Img/bag2.png");
            _texture2 = Texture.LoadFromFile("Resources/Img/bag3.png");

            //每0.1秒进行一次变换
            _computeTimer = new System.Threading.Timer(t =>
            {
                minVal = minVal + change;
            }, null, 0, 100);
        }
    }
}
