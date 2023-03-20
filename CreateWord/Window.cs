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

namespace LearnOpenTK
{
    /// <summary>
    /// 练习三角形 四边形
    /// </summary>
    public class Window : GameWindow
    {
        private float frameTime = 0.0f;
        private int fps = 0;

        private readonly float[] _vertices1 =
        {
            -0.5f, -0.5f, 0.0f, // 下左顶点
             0.5f, -0.5f, 0.0f, // 下右顶点
             0.0f,  0.5f, 0.0f  // 上顶点
        };
        private readonly float[] _vertices2 =
{
             0.5f,  0.5f, 0.0f, // 右上角
             0.5f, -0.5f, 0.0f, // 右下角
            -0.5f, -0.5f, 0.0f, // 左下角
            -0.5f,  0.5f, 0.0f, // 左上角
        };

        private readonly uint[] _indices =
        {
            0, 1, 3, 
            1, 2, 3
        };

        private readonly float[] _vertices3 =
{
             0.5f,  0.5f, 0.0f,
             0.5f, -0.5f, 0.0f,
            -0.5f,  0.5f, 0.0f,
             0.5f, -0.5f, 0.0f,
            -0.5f, -0.5f, 0.0f,
            -0.5f,  0.5f, 0.0f,

        };

        private int _vertexArrayObject1;

        private int _vertexBufferObject1;

        private int _vertexArrayObject2;

        private int _vertexBufferObject2;

        private int _elementBufferObject;

        private int _vertexArrayObject3;

        private int _vertexBufferObject3;


        private Shader _shader;
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
        }
        protected override void OnLoad()
        {
            base.OnLoad();
            //设置清空屏幕所用的颜色
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            _vertexBufferObject1 = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject1);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices1.Length * sizeof(float), _vertices1, BufferUsageHint.StaticDraw);
            //OpenGL是一个状态机，目前绑定到GL_ARRAY_BUFFER上（BufferTarget.ArrayBuffer）
            _vertexArrayObject1 = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject1);
            //VertexAttribPointer通过当前绑定到GL_ARRAY_BUFFER的VBO
            //参数1是代表着色器位置 layout(location = 0)
            //参数2是指定顶点属性的大小，目前是三个值组合成的顶点，所以是3
            //参数3是顶点的类型，目前是Float
            //参数4是代表是否希望数据被标准化，标准化则数据范围在0-1之间，目前我们用到-1.0，所以是false
            //参数5是顶点数据的空间步长，即顶点占用多少字节
            //参数6是顶点数据之间的偏移，我们数据是整齐的中间没有其他，则为0
            //用来给这个location提供数据（提供VBO的属性，比如位置信息）
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            //OpenGL是状态机，所以要给每个VAO对象指定位置，这里是VAO1
            GL.EnableVertexAttribArray(0);

            _vertexBufferObject2 = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject2);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices2.Length * sizeof(float), _vertices2, BufferUsageHint.StaticDraw);
            _vertexArrayObject2 = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject2);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

            _vertexBufferObject3 = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject3);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices3.Length * sizeof(float), _vertices3, BufferUsageHint.StaticDraw);
            _vertexArrayObject3 = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject3);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

            //启用着色器位置 layout(location = 0)
            //OpenGL是状态机，所以要给每个VAO对象指定位置，这里是VAO2
            GL.EnableVertexAttribArray(0);
            //GL.EnableVertexAttribArray(1);

            //EBOd的绑定方式和VAO类似，它是绑定顶点位置的索引
            _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            _shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");
            _shader.Use();

            //解除状态机的绑定
            //GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            //GL.BindVertexArray(0);
        }

        // This function runs on every update frame.
        // 每帧运行该函数
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            FPS(e);

            var key = KeyboardState;

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
            _shader.Use();
            if (DateTime.Now.Second % 10 > 5)//两个VAO切换
            {
                GL.BindVertexArray(_vertexArrayObject1);
                //OpenGL内基本就是点、线、三角形
                //参数1是绘制的图元
                //参数2是顶点数组的起始索引
                //参数3是绘制的顶点数量
                GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
            }
            else {
                //GL.BindVertexArray(_vertexArrayObject2);
                // 通知是以索引的方式绘制，类型是三角形、索引的长度、索引的类型、偏移默认是0
                //GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);

                GL.BindVertexArray(_vertexArrayObject3);
                GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            }
            base.SwapBuffers();//交换缓冲，建议最后
        }

        protected override void OnUnload() {
            // Unbind all the resources by binding the targets to 0/null.
            // 通过绑定0/null来取消所有资源
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            // Delete all the resources.
            // 删除资源
            GL.DeleteBuffer(_vertexBufferObject1);
            GL.DeleteBuffer(_vertexBufferObject2);
            GL.DeleteVertexArray(_vertexArrayObject1);
            //GL.UseProgram(0);
            Shader.Clear();
            // 删除着色器
            //GL.DeleteProgram(_shader.Handle);
            _shader.Remove();

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
