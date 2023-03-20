using System;
using System.Diagnostics;
using System.Collections.Generic;
using OpenTK;
//using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using LearnOpenTK.Common;

namespace LearnOpenTK
{

    /// <summary>
    /// 练习纹理贴图
    /// </summary>
    public class Window : GameWindow
    {
        private float frameTime = 0.0f;
        private int fps = 0;

        private readonly float[] _vertices =
        {
            // Position         Texture coordinates
             0.5f,  0.5f, 0.0f, 1.0f, 1.0f, // 右上角 top right 图片是结束点
             0.5f, -0.5f, 0.0f, 1.0f, 0.0f, // 右下角 bottom right
            -0.5f, -0.5f, 0.0f, 0.0f, 0.0f, // 左下角 bottom left 图片是开始点
            -0.5f,  0.5f, 0.0f, 0.0f, 1.0f  // 左上角 top left
        };

        private readonly uint[] _indices = 
        {
            0, 1, 3,
            1, 2, 3
        };

        private int _elementBufferObject;

        private int _vertexBufferObject;

        private int _vertexArrayObject;

        private Shader _shader;
        private Texture _texture;
        private Texture _texture1;

        //private Matrix4 _view;

        //private Matrix4 _projection;

        // The view and projection matrices have been removed as we don't need them here anymore.
        // They can now be found in the new camera class.

        // We need an instance of the new camera class so it can manage the view and projection matrix code.
        // We also need a boolean set to true to detect whether or not the mouse has been moved for the first time.
        // Finally, we add the last position of the mouse so we can calculate the mouse offset easily.
        private Camera _camera;


        private bool _firstMove = true;

        private Vector2 _lastPos;

        private double _time;
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
            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            _shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");
            //_shader.Use();

            //给aPosition这个location提供数据，这里是提供坐标位置
            var vertexLocation = _shader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            //给aTexCoord这个location提供数据，这里是提供纹理的坐标，它在VBO的后两位，需要偏移3位
            //纹理的一张图片，图片是2D的，只要确定两个点，就可以确定平面上的位置
            //纹理坐标起始于(0, 0)，也就是纹理图片的左下角，终始于(1, 1)，即纹理图片的右上角
            var texCoordLocation = _shader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

            _texture = Texture.LoadFromFile("Resources/container.png");
            //_texture.Use(TextureUnit.Texture0);
            _texture1 = Texture.LoadFromFile("Resources/awesomeface.png");
            //_texture1.Use(TextureUnit.Texture1);
            //默认纹理单元是0，顺序是一次累加


            // 注意乘法要从右向左读
            //gl_Position = projection * view * model * vec4(aPos, 1.0);
            //gl_Position = vec4(aPosition, 1.0) * model * view * projection;
            /*
            它的第一个参数定义了fov的值，它表示的是视野(Field of View)，并且设置了观察空间的大小。
            如果想要一个真实的观察效果，它的值通常设置为45.0f，但想要一个末日风格的结果你可以将其设置一个更大的值。
            第二个参数设置了宽高比，由视口的宽除以高所得。
            第三和第四个参数设置了平截头体的近和远平面。我们通常设置近距离为0.1f，而远距离设为100.0f。
            所有在近平面和远平面内且处于平截头体内的顶点都会被渲染。
             */
            /*
             当你把透视矩阵的 near 值设置太大时（如10.0f），OpenGL会将靠近摄像机的坐标（在0.0f和10.0f之间）都裁剪掉，
            这会导致一个你在游戏中很熟悉的视觉效果：在太过靠近一个物体的时候你的视线会直接穿过去。
             */
            //它是观察的一个视角设定
            //_projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90f), Size.X / (float)Size.Y, 0.1f, 100.0f);
            //_view = Matrix4.CreateTranslation(0.0f, 0.0f, -3.0f);
            _shader.SetInt("texture0", 0);
            _shader.SetInt("texture1", 1);

            //Vector3.UnitZ(0,0,1)
            //Vector3.UnitX(1,0,0)
            //Vector3.UnitY(0,1,0)
            _camera = new Camera(Vector3.UnitZ * 3, Size.X / (float)Size.Y);

        }

        // 每帧运行该函数
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            FPS(e);

            var key = base.KeyboardState;

            if (key.IsKeyDown(Keys.Escape))
            {
                Close();
            }

            CameraOp(e);
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

            //循环绘制
            for (int i = 0; i < 3; i++)
            {
                var model = Matrix4.Identity *
                    Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(i * 15));
                _shader.SetMatrix4("model", model, true);
                _shader.SetMatrix4("view", _camera.GetViewMatrix(), true);
                _shader.SetMatrix4("projection", _camera.GetProjectionMatrix(), true);

                _shader.Use();
                _texture.Use(TextureUnit.Texture0);
                _texture1.Use(TextureUnit.Texture1);

                //开始绘制
                GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
            }
            //单位矩阵 Matrix4.Identity
            // 如果将各个矩阵传递给着色器并在那里相乘，则必须按照“模型*视图*投影”的顺序进行

            base.SwapBuffers();//交换缓冲，建议最后
        }

        protected override void OnUnload() {
            // Unbind all the resources by binding the targets to 0/null.
            // 通过绑定0/null来取消所有资源
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            // Delete all the resources.
            // 删除资源
            GL.DeleteBuffer(_vertexBufferObject);
            //GL.DeleteBuffer(_vertexBufferObject2);
            GL.DeleteVertexArray(_vertexArrayObject);
            //GL.UseProgram(0);
            Shader.Clear();
            // 删除着色器
            //GL.DeleteProgram(_shader.Handle);
            _texture.Remove();
            _texture1.Remove();
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

        private void CameraOp(FrameEventArgs e) {
            var input = base.KeyboardState;
            // 移动作的速度 
            const float cameraSpeed = 1.5f;
            // 灵敏度
            const float sensitivity = 0.2f;

            // 移动Z，感观上的前后远近
            if (input.IsKeyDown(Keys.W))
            {
                _camera.Position += _camera.Front * cameraSpeed * (float)e.Time; // Forward
            }
            if (input.IsKeyDown(Keys.S))
            {
                _camera.Position -= _camera.Front * cameraSpeed * (float)e.Time; // Backwards
            }
            // X轴移动
            if (input.IsKeyDown(Keys.A))
            {
                _camera.Position -= _camera.Right * cameraSpeed * (float)e.Time; // Left
            }
            if (input.IsKeyDown(Keys.D))
            {
                _camera.Position += _camera.Right * cameraSpeed * (float)e.Time; // Right
            }

            //感观上的上下移动
            if (input.IsKeyDown(Keys.Space))
            {
                _camera.Position += _camera.Up * cameraSpeed * (float)e.Time; // Up
            }
            if (input.IsKeyDown(Keys.LeftShift))
            {
                _camera.Position -= _camera.Up * cameraSpeed * (float)e.Time; // Down
            }

            var mouse = base.MouseState;

            if (_firstMove)//鼠标第一次
            {
                _lastPos = new Vector2(mouse.X, mouse.Y);
                _firstMove = false;
            }
            else
            {
                var deltaX = mouse.X - _lastPos.X;
                var deltaY = mouse.Y - _lastPos.Y;
                _lastPos = new Vector2(mouse.X, mouse.Y);

                // Apply the camera pitch and yaw (we clamp the pitch in the camera class)
                _camera.Yaw += deltaX * sensitivity;
                _camera.Pitch -= deltaY * sensitivity; // Reversed since y-coordinates range from bottom to top
            }
        }

    }
}
