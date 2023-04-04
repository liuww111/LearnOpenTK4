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

        private ImagePlus _imagePlus;

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

            Init();
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
                _imagePlus.MovePosition(_imagePlus.X, _imagePlus.Y + step);
            }
            if (key.IsKeyDown(Keys.S))
            {
                _imagePlus.MovePosition(_imagePlus.X, _imagePlus.Y - step);
            }
            if (key.IsKeyDown(Keys.A))
            {
                _imagePlus.MovePosition(_imagePlus.X - step, _imagePlus.Y);
            }
            if (key.IsKeyDown(Keys.D))
            {
                _imagePlus.MovePosition(_imagePlus.X + step, _imagePlus.Y);
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

            _imagePlus.Use();

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

        private void Init() {
            _shader = new Shader("Shaders/tileShader.vert", "Shaders/tileShader.frag");
            _shader.SetMatrix4("projection", Shader.GLMOrthographic(0.0f, Size.X, 0.0f, Size.Y));//投影的场景大小

            _imagePlus = ImagePlus.Load("Resources/Img/bag3.png", _shader);
        }
    }
}
