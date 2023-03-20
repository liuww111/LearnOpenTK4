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

    public class Window : GameWindow
    {
        private float frameTime = 0.0f;
        private int fps = 0;

        private int _vertexBufferObject;

        private int _vertexArrayObject;

        private Shader _shader;
        private Texture _texture;

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
            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            //2D 平面，每个角只需要x,y，由两个三角形组成
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * 6 * 4, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 0, 0);

            _shader = new Shader("Shaders/tileShader.vert", "Shaders/tileShader.frag");
            _shader.SetMatrix4("projection", Shader.GLMOrthographic(0.0f, Size.X, 0.0f, Size.Y));//投影的场景大小

            _texture = Texture.LoadFromFile("Resources/Tile/Tile4.png");
            //_texture.Use(TextureUnit.Texture0);

            
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

            GL.BindVertexArray(_vertexArrayObject);

            /*
            model：模型自己的位置（局部空间），一般调整方位，比如靠旋转
            view：模型在世界空间内的位置，视角内观察到的位置
            projection：模型所在的世界空间裁剪的矩阵（世界空间）
            */

            //循环绘制
            _texture.Use(TextureUnit.Texture0);
            _shader.Use();

            int r = Size.Y / _texture.Height;
            int c= Size.X / _texture.Width;

            //开始绘制
            for (int row = 0; row < r; row++) 
            {
                for (int column=0; column<c; column++) {
                    var vertices = TileVertices(column * _texture.Width, row * _texture.Height, _texture.Width, _texture.Height);
                    //GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicDraw);
                    GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, sizeof(float)* vertices.Length, vertices);
                    GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
                }
            }

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

        /// <summary>
        /// 贴片的顶角
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        private float[] TileVertices(float x,float y,float width,float height,float scale=1.0f) {
            float xpos = x ;
            float ypos = y ;
            float w = width * scale;
            float h = height * scale;

            return  new float[]
            {
                    xpos, ypos+h, 0.0f, 0.0f,
                    xpos, ypos, 0.0f, 1.0f,
                    xpos + w, ypos, 1.0f, 1.0f,

                    xpos, ypos+h, 0.0f, 0.0f,
                    xpos + w, ypos, 1.0f, 1.0f,
                    xpos + w, ypos+h, 1.0f, 0.0f
            };
        }
    }
}
