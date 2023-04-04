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
    /// 多着色器多纹理
    /// </summary>
    public class Window : GameWindow
    {

        private Shader _shader;
        
        private Shader _shader2;

        private TexturePlus _texture1;

        private TexturePlus _texture2;

        private TexturePlus _texture3;

        private ImagePlus _texture4;

        private System.Threading.Timer _computeTimer;

        private static decimal minVal = 0.0m;

        private static decimal change = 0.1m;

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            var Version = GL.GetString(StringName.Version);//OpenGL版本
            var Vendor = GL.GetString(StringName.Vendor);
            var Renderer = GL.GetString(StringName.Renderer);//渲染器 显卡
            var ShadingLanguageVersion = GL.GetString(StringName.ShadingLanguageVersion);//GLSL版本

            Debug.WriteLine($"Version:{Version} Vendor:{Vendor} Renderer:{Renderer} GLSL:{ShadingLanguageVersion}");
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
            int step = 10;
            if (key.IsKeyDown(Keys.Escape))
            {
                Close();
            }
            if (key.IsKeyDown(Keys.W))
            {
                _texture1.MovePosition(_texture1.X, _texture1.Y + step);
                _texture2.MovePosition(_texture2.X, _texture2.Y + step);
            }
            if (key.IsKeyDown(Keys.S))
            {
                _texture1.MovePosition(_texture1.X, _texture1.Y - step);
                _texture2.MovePosition(_texture2.X, _texture2.Y - step);
            }
            if (key.IsKeyDown(Keys.A))
            {
                _texture1.MovePosition(_texture1.X - step, _texture1.Y);
                _texture2.MovePosition(_texture2.X - step, _texture2.Y);
            }
            if (key.IsKeyDown(Keys.D))
            {
                _texture1.MovePosition(_texture1.X + step, _texture1.Y);
                _texture2.MovePosition(_texture2.X + step, _texture2.Y);
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
            //GL.BindVertexArray(_vertexArrayObject);

            //Debug.WriteLine($"mixVal:{minVal % 1} ---- {(int)(minVal % 4)}");
            _shader.SetFloat("mixVal", Convert.ToSingle(minVal % 1));
            //_shader.SetFloat("mixVal", 0.5f);
            //_texture2 = TexturePlus.LoadFromFile($"Resources/Img/bag{(int)(minVal % 2)}.png");
            _shader.Use();

            _texture1.Use(unit: TextureUnit.Texture0);
            _texture2.Use(unit: TextureUnit.Texture1);

            //_shader2.Use();
            //_texture3.Use(unit: TextureUnit.Texture0);

            _texture4.Use();

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

            _shader = new Shader("Shaders/mixShader.vert", "Shaders/mixShader.frag");
            // 指定采样器的取的纹理单元
            _shader.SetInt("texture0", 0);
            _shader.SetInt("texture1", 1);
            _shader.SetMatrix4("projection", Shader.GLMOrthographic(0.0f, Size.X, 0.0f, Size.Y));//投影的场景大小

            _shader2 = new Shader("Shaders/tileShader.vert", "Shaders/tileShader.frag");
            _shader2.SetMatrix4("projection", Shader.GLMOrthographic(0.0f, Size.X, 0.0f, Size.Y));//投影的场景大小

            _texture1 = TexturePlus.LoadFromFile("Resources/Img/bag0.png");
            _texture2 = TexturePlus.LoadFromFile("Resources/Img/bag2.png");

            _texture3 = TexturePlus.LoadFromFile("Resources/Img/bag1.png");

            _texture4 = ImagePlus.Load("Resources/Img/bag3.png", _shader2);

            //每0.1秒进行一次变换
            _computeTimer = new System.Threading.Timer(t =>
            {
                minVal = minVal + change;
            }, null, 0, 200);
        }
    }
}
