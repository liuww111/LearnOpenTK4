using LearnOpenTK.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using System;
using System.Diagnostics;
using FreeTypeSharp;
using FreeTypeSharp.Native;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace LearnOpenTK
{
    // This is where all OpenGL code will be written.
    // OpenToolkit allows for several functions to be overriden to extend functionality; this is how we'll be writing code.
    // 此处编写OpenGl
    // 可以扩展重新函数
    public class Window : GameWindow
    {
        // A simple constructor to let us set properties like window size, title, FPS, etc. on the window.
        //设置窗口大小、标题、FPS
        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
        }
 
        
        private int _vertexBufferObject;
        private int _vertexArrayObject;
 
        protected override void OnLoad()
        {
            base.OnLoad();

            //GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

 

            _vertexArrayObject = GL.GenVertexArray();
            _vertexBufferObject = GL.GenBuffer();
            //绑定VAO
            GL.BindVertexArray(_vertexArrayObject);
            //绑定VBO
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            //每个2D四边形需要6个顶点，每个顶点又是由一个4float向量（译注：一个纹理坐标和一个顶点坐标）组成
            //因此我们将VBO的内存分配为6 * 4个float的大小
            GL.BufferData(BufferTarget.ArrayBuffer,  sizeof(float) * 6 * 4 , IntPtr.Zero, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4*sizeof(float), 0);
            //清除绑定设置
            GL.BindBuffer(BufferTarget.ArrayBuffer,0);
            GL.BindVertexArray(0);
        }

        // This function runs on every update frame.
        // 每帧运行该函数
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            // Check if the Escape button is currently being pressed.
            //按下esc键后退出
            //KeyboardState内封装了对键盘的一些操作函数
            if (KeyboardState.IsKeyDown(Keys.Escape))//判断键是否按下
            {
                // If it is, close the window.
                base.Close();
            }
            //扩展
            //IsKeyPressed  
            //IsKeyReleased 
            //GetSnapshot
            //WasKeyDown  
            base.OnUpdateFrame(e);
        }

        /*
OpenTK.Windowing.Desktop.GameWindow.OnLoad：在创建OpenGL后发生

//但是在进入主循环之前。覆盖以加载资源。

//OpenTK.Windowing.Desktop.GameWindow.OnUnload:退出主循环后发生，

//但在删除OpenGL上下文之前。重写以卸载资源。

//OpenTK.Windowing.Desktop.NativeWindow.OnResize（OpenTK.WWindowing.Common.ResizeEventArgs）：

//每当GameWindow调整大小时发生。您应该在此处更新OpenGL视口和投影矩阵。

//OpenTK.Windowing.Desktop.GameWindow.OnUpdateFrame（OpenTK.WWindowing.Common.FrameEventArgs）：

//以指定的逻辑更新速率发生。覆盖以添加游戏逻辑。

//OpenTK.Windowing.Desktop.GameWindow.OnRenderFrame（OpenTK.WWindowing.Common.FrameEventArgs）：

//以指定的帧呈现速率发生。覆盖以添加渲染代码。
        */

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);
 

            base.SwapBuffers();
        }

 

    }
}
