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
    /// 练习文本渲染
    /// </summary>
    public class Window : GameWindow
    {
        private float frameTime = 0.0f;
        private int fps = 0;
 
        private FontManage _fontManage;

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
            GL.Enable(EnableCap.Blend);
        }
        protected override void OnLoad()
        {
            base.OnLoad();
            //设置清空屏幕所用的颜色
            GL.ClearColor(0.9f, 0.9f, 0.9f, 1.0f);

            //GL.Enable(EnableCap.CullFace);
            //通过启用混合，让背景保持透明
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);


            _fontManage = new FontManage(Size.X, Size.Y, @"./Resources/Fonts/STKAITI.TTF", "./Resources/stringFont.txt");

            //VSync = VSyncMode.Off;

             
        }

        /// <summary>
        /// 每帧运行该函数
        /// </summary>
        /// <param name="e"></param>
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            FPS(e);

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

            var _fps = Math.Round(1d / e.Time);
            _fontManage.PrintText(_fps.ToString() + "FPS,我是谁？", 0f, 600f-48f, 1f, new Vector3(0.8f, 0.2f, 0.1f));
            _fontManage.PrintText("This is sample text", 25.0f, 25.0f, 1f, new Vector3(0.5f, 0.8f, 0.2f));
            _fontManage.PrintText("(C) LearnOpenGL.com", 540.0f, 570.0f, 0.5f, new Vector3(0.3f, 0.7f, 0.9f));

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

        //显示帧数
        private void FPS(FrameEventArgs e) {
            frameTime += (float)e.Time;
            fps++;
            if (frameTime >= 1.0f)
            {
                base.Title = $"OpenTK {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} : FPS - {fps}";
                frameTime = 0.0f;
                fps = 0;
            }
        }

    }
}
