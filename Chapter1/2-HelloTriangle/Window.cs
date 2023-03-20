using LearnOpenTK.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;

namespace LearnOpenTK
{
    // Be warned, there is a LOT of stuff here. It might seem complicated, but just take it slow and you'll be fine.
    // OpenGL's initial hurdle is quite large, but once you get past that, things will start making more sense.
    public class Window : GameWindow
    {
        // Create the vertices for our triangle. These are listed in normalized device coordinates (NDC)
        // In NDC, (0, 0) is the center of the screen.
        // Negative X coordinates move to the left, positive X move to the right.
        // Negative Y coordinates move to the bottom, positive Y move to the top.
        // OpenGL only supports rendering in 3D, so to create a flat triangle, the Z coordinate will be kept as 0.
        /*
        为三角形创建顶点。这些列在标准化设备坐标（NDC）中
        在NDC中，（0，0）是屏幕的中心。
        负X坐标向左移动，正X坐标向右移动。
        负Y坐标移至底部，正Y坐标移至顶部。
        OpenGL仅支持3D渲染，因此要创建平面三角形，Z坐标将保持为0。
         */
        private readonly float[] _vertices =
        {
            -0.5f, -0.5f, 0.0f, // Bottom-left vertex
             0.5f, -0.5f, 0.0f, // Bottom-right vertex
             0.0f,  0.5f, 0.0f  // Top vertex
        };

        // These are the handles to OpenGL objects. A handle is an integer representing where the object lives on the
        // graphics card. Consider them sort of like a pointer; we can't do anything with them directly, but we can
        // send them to OpenGL functions that need them.

        // What these objects are will be explained in OnLoad.
        private int _vertexBufferObject;

        private int _vertexArrayObject;

        // This class is a wrapper around a shader, which helps us manage it.
        // The shader class's code is in the Common project.
        // What shaders are and what they're used for will be explained later in this tutorial.
        /*
        这个类是着色器的包装器
        着色器类的代码在Common项目中。
        GLSL教程参考：
        https://www.cnblogs.com/zhxmdefj/p/11241537.html
        https://github.com/wshxbqq/GLSL-Card
        https://www.jianshu.com/p/66b10062bd67
        https://www.cnblogs.com/brainworld/p/7445290.html
         */
        private Shader _shader;

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        // Now, we start initializing OpenGL.
        protected override void OnLoad()
        {
            base.OnLoad();

            // This will be the color of the background after we clear it, in normalized colors.
            // Normalized colors are mapped on a range of 0.0 to 1.0, with 0.0 representing black, and 1.0 representing
            // the largest possible value for that channel.
            // This is a deep green.
            // OpenGL颜色体系
            // 参考文章 https://www.cnblogs.com/tjulym/p/5037124.html
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            // We need to send our vertices over to the graphics card so OpenGL can use them.
            // To do this, we need to create what's called a Vertex Buffer Object (VBO).
            // These allow you to upload a bunch of data to a buffer, and send the buffer to the graphics card.
            // This effectively sends all the vertices at the same time.

            //OpenGL有自己的缓冲区

            // First, we need to create a buffer. This function returns a handle to it, but as of right now, it's empty.
            // 创建一段缓冲对象
            _vertexBufferObject = GL.GenBuffer();

            // Now, bind the buffer. OpenGL uses one global state, so after calling this,
            // all future calls that modify the VBO will be applied to this buffer until another buffer is bound instead.
            // The first argument is an enum, specifying what type of buffer we're binding. A VBO is an ArrayBuffer.
            // There are multiple types of buffers, but for now, only the VBO is necessary.
            // The second argument is the handle to our buffer.
            //将这个VBO绑定到这个类型的缓冲区
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);

            // Finally, upload the vertices to the buffer.
            // Arguments:
            //   Which buffer the data should be sent to.
            //   How much data is being sent, in bytes. You can generally set this to the length of your array, multiplied by sizeof(array type).
            //   The vertices themselves.
            //   How the buffer will be used, so that OpenGL can write the data to the proper memory space on the GPU.
            //   There are three different BufferUsageHints for drawing:
            //     StaticDraw: This buffer will rarely, if ever, update after being initially uploaded.
            //     DynamicDraw: This buffer will change frequently after being initially uploaded.
            //     StreamDraw: This buffer will change on every frame.
            //   Writing to the proper memory space is important! Generally, you'll only want StaticDraw,
            //   but be sure to use the right one for your use case.
            //BufferUsageHint 代表缓冲区的使用方式，StaticDraw：不频繁使用，偏向静态，DynamicDraw：频繁使用，偏向动态，StreamDraw：每帧更改
            //给这个缓冲区绑定数据
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);
            //通过BufferTarget.ArrayBuffer，VBO和_vertices进行了关联


            //总结 VBO就是各个顶点数据存放的位置，是个指针
            //首先是先创建对象，然后绑定到缓冲区（BindBuffer函数，缓冲区类型BufferTarget.ArrayBuffer），
            //接着给这个缓冲区绑定数据（BufferData函数，缓冲区类型BufferTarget.ArrayBuffer）

            // One notable thing about the buffer we just loaded data into is that it doesn't have any structure to it. It's just a bunch of floats (which are actaully just bytes).
            // The opengl driver doesn't know how this data should be interpreted or how it should be divided up into vertices. To do this opengl introduces the idea of a 
            // Vertex Array Obejct (VAO) which has the job of keeping track of what parts or what buffers correspond to what data. In this example we want to set our VAO up so that 
            // it tells opengl that we want to interpret 12 bytes as 3 floats and divide the buffer into vertices using that.
            // To do this we generate and bind a VAO (which looks deceptivly similar to creating and binding a VBO, but they are different!).
            //缓冲区是无结构的，opengl有VAO对象，是跟踪哪些部分或哪些缓冲区对应于哪些数据
            //将12个字节解释为3个浮点
            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            // Now, we need to setup how the vertex shader will interpret the VBO data; you can send almost any C datatype (and a few non-C ones too) to it.
            // While this makes them incredibly flexible, it means we have to specify how that data will be mapped to the shader's input variables.

            // To do this, we use the GL.VertexAttribPointer function
            // This function has two jobs, to tell opengl about the format of the data, but also to associate the current array buffer with the VAO.
            // This means that after this call, we have setup this attribute to source data from the current array buffer and interpret it in the way we specified.
            // Arguments:
            //   Location of the input variable in the shader. the layout(location = 0) line in the vertex shader explicitly sets it to 0.
            //   How many elements will be sent to the variable. In this case, 3 floats for every vertex.
            //   The data type of the elements set, in this case float.
            //   Whether or not the data should be converted to normalized device coordinates. In this case, false, because that's already done.
            //   The stride; this is how many bytes are between the last element of one vertex and the first element of the next. 3 * sizeof(float) in this case.
            //   The offset; this is how many bytes it should skip to find the first element of the first vertex. 0 as of right now.
            // Stride and Offset are just sort of glossed over for now, but when we get into texture coordinates they'll be shown in better detail.
            //顶点数据的解析方式以Float类型从0开始读取，每3个一组，大小是12，偏移为0
            //每个顶点属性从一个VBO管理的内存中获得它的数据，
            //而具体是从哪个VBO（程序中可以有多个VBO）获取则是通过在调用glVertexAttribPointer时绑定到GL_ARRAY_BUFFER的VBO决定的
            //他们通过GL_ARRAY_BUFFER建立联系
            //第一个参数0，也代表着色器的位置layout(location = 0)，和EnableVertexAttribArray呼应，他们是联系在一起的
            //VAO是VBO的管理方式，创建Pointer会存在VAO中
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

            //总结 VAO就是管理操作VBO的Pointer
            //先是创建它的对象，然后是绑定到缓冲区
            //接着告诉VAO这个VBO内数据的结构，也就是读取的方式

            // Enable variable 0 in the shader.
            // 启用着色器的位置是0
            // 色器源码里面的layout (location = 0)
            //0是我们所需要的信息的位置，glEnableVertexAttribArray(0);是将VBO这系列操作放在0
            GL.EnableVertexAttribArray(0);

            //VAO存储以下内容：
            //EnableVertexAttribArray和DisableVertexAttribArray的调用。
            //通过VertexAttribPointer设置的顶点属性配置。
            //通过VertexAttribPointer调用与顶点属性关联的顶点缓冲对象。
            //所以VAO的创建和绑定必须在VertexAttribPointer和EnableVertexAttribArray之前


            // We've got the vertices done, but how exactly should this be converted to pixels for the final image?
            // Modern OpenGL makes this pipeline very free, giving us a lot of freedom on how vertices are turned to pixels.
            // The drawback is that we actually need two more programs for this! These are called "shaders".
            // Shaders are tiny programs that live on the GPU. OpenGL uses them to handle the vertex-to-pixel pipeline.
            // Check out the Shader class in Common to see how we create our shaders, as well as a more in-depth explanation of how shaders work.
            // shader.vert and shader.frag contain the actual shader code.
            //顶点着色器，片段着色器
            _shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");

            // Now, enable the shader.
            // Just like the VBO, this is global, so every function that uses a shader will modify this one until a new one is bound instead.
            _shader.Use();

            // Setup is now complete! Now we move to the OnRenderFrame function to finally draw the triangle.
        }

        // Now that initialization is done, let's create our render loop.
        //渲染的主循环
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            // This clears the image, using what you set as GL.ClearColor earlier.
            // OpenGL provides several different types of data that can be rendered.
            // You can clear multiple buffers by using multiple bit flags.
            // However, we only modify the color, so ColorBufferBit is all we need to clear.
            //根据不同的渲染类型选择不同的清空方式
            GL.Clear(ClearBufferMask.ColorBufferBit);
            //因为在渲染循环里，你是一个在不断绘制图像的过程，如果不清屏，那么每次画的图像就会叠加，

            // To draw an object in OpenGL, it's typically as simple as binding your shader,
            // setting shader uniforms (not done here, will be shown in a future tutorial)
            // binding the VAO,
            // and then calling an OpenGL function to render.

            // Bind the shader
            _shader.Use();

            // Bind the VAO
            GL.BindVertexArray(_vertexArrayObject);

            // And then call our drawing function.
            // For this tutorial, we'll use GL.DrawArrays, which is a very simple rendering function.
            // Arguments:
            //   Primitive type; What sort of geometric primitive the vertices represent.
            //     OpenGL used to support many different primitive types, but almost all of the ones still supported
            //     is some variant of a triangle. Since we just want a single triangle, we use Triangles.
            //   Starting index; this is just the start of the data you want to draw. 0 here.
            //   How many vertices you want to draw. 3 for a triangle.
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

            // OpenTK windows are what's known as "double-buffered". In essence, the window manages two buffers.
            // One is rendered to while the other is currently displayed by the window.
            // This avoids screen tearing, a visual artifact that can happen if the buffer is modified while being displayed.
            // After drawing, call this function to swap the buffers. If you don't, it won't display what you've rendered.
            //交换缓冲区
            base.SwapBuffers();
            /*
             因为OPENGL是双缓冲，那么双缓冲的作用是什么呢？
             假如是单缓冲，由于屏幕上的像素绘制是由左到右，从上到下一个一个绘制的，如果缓冲量过大，那么可能会出现图像闪烁，
             但是如果是双缓冲，它是由前缓冲全部绘制好，然后再跟后缓冲互换，然后前后工作交替进行，这样子就解决了闪烁问题。
             */
            // And that's all you have to do for rendering! You should now see a yellow triangle on a black screen.
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            var input = KeyboardState;

            if (input.IsKeyDown(Keys.Escape))
            {
                Close();
            }
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            // When the window gets resized, we have to call GL.Viewport to resize OpenGL's viewport to match the new size.
            // If we don't, the NDC will no longer be correct.
            //调用GL.Viewport调整渲染管道的大小
            GL.Viewport(0, 0, Size.X, Size.Y);
        }

        // Now, for cleanup.
        // You should generally not do cleanup of opengl resources when exiting an application,
        // as that is handled by the driver and operating system when the application exits.
        // 
        // There are reasons to delete opengl resources, but exiting the application is not one of them.
        // This is provided here as a reference on how resource cleanup is done in opengl, but
        // should not be done when exiting the application.
        //
        // Places where cleanup is appropriate would be: to delete textures that are no
        // longer used for whatever reason (e.g. a new scene is loaded that doesn't use a texture).
        // This would free up video ram (VRAM) that can be used for new textures.
        //
        // The coming chapters will not have this code.
        protected override void OnUnload()
        {
            // Unbind all the resources by binding the targets to 0/null.
            // 通过绑定0/null来取消所有资源
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            // Delete all the resources.
            // 删除资源
            GL.DeleteBuffer(_vertexBufferObject);
            GL.DeleteVertexArray(_vertexArrayObject);

            GL.DeleteProgram(_shader.Handle);

            base.OnUnload();
        }
    }
}
