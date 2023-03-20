using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace LearnOpenTK.Common
{
    // A simple class meant to help create shaders.
    public class Shader
    {
        public readonly int Handle;

        private readonly Dictionary<string, int> _uniformLocations;

        // This is how you create a simple shader.
        // Shaders are written in GLSL, which is a language very similar to C in its semantics.
        // The GLSL source is compiled *at runtime*, so it can optimize itself for the graphics card it's currently being used on.
        // A commented example of GLSL can be found in shader.vert.
        /*
        这是创建简单着色器的方法。
        着色器是用GLSL编写的，GLSL是一种在语义上与C非常相似的语言。
        GLSL源代码是在运行时*编译的，因此它可以针对当前使用的图形卡进行自我优化。
        GLSL的注释示例可以在shader.vert中找到。.vert文件是着色文件
         */
        //注意：着色器代码不能添加中文注释，添加后无法编译通过
        public Shader(string vertPath, string fragPath)
        {
            // There are several different types of shaders, but the only two you need for basic rendering are the vertex and fragment shaders.
            // The vertex shader is responsible for moving around vertices, and uploading that data to the fragment shader.
            //   The vertex shader won't be too important here, but they'll be more important later.
            // The fragment shader is responsible for then converting the vertices to "fragments", which represent all the data OpenGL needs to draw a pixel.
            //   The fragment shader is what we'll be using the most here.

            // Load vertex shader and compile
            // 加载顶点着色器
            var shaderSource = File.ReadAllText(vertPath);

            // GL.CreateShader will create an empty shader (obviously). The ShaderType enum denotes which type of shader will be created.
            // 创建着色器的类型，目前是六种着色器
            var vertexShader = GL.CreateShader(ShaderType.VertexShader);//顶点着色器

            // Now, bind the GLSL source code
            // 着色器实例与着色器代码绑定,就像该实例装载该着色器的逻辑脚本一样
            GL.ShaderSource(vertexShader, shaderSource);

            // And then compile 编辑成机器码
            Shader.CompileShader(vertexShader);

            // We do the same for the fragment shader.
            shaderSource = File.ReadAllText(fragPath);
            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);//片段着色器
            GL.ShaderSource(fragmentShader, shaderSource);
            Shader.CompileShader(fragmentShader);//编译

            // These two shaders must then be merged into a shader program, which can then be used by OpenGL.
            // To do this, create a program...
            //创建处理着色器的应用，它是要在GPU上运行的程序
            Handle = GL.CreateProgram();

            // Attach both shaders...
            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);

            // And then link them together.
            // 链接他们，这本身的操作就像编译C++一样，加载运行库，链接依赖的库，生成一个程序运行的环境，即运行上下文
            Shader.LinkProgram(Handle);

            // When the shader program is linked, it no longer needs the individual shaders attached to it; the compiled code is copied into the shader program.
            // Detach them, and then delete them.
            // 当着色器程序被链接后，程序就可以上载到GPU，之后就可以卸载和删除，释放资源
            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, fragmentShader);
            GL.DeleteShader(fragmentShader);
            GL.DeleteShader(vertexShader);

            // The shader is now ready to go, but first, we're going to cache all the shader uniform locations.
            // Querying this from the shader is very slow, so we do it once on initialization and reuse those values
            // later.

            // First, we have to get the number of active uniforms in the shader.
            // uniform是全局的，它称为常量存储，这是一种分配在硬件上的存储常量值的空间
            // 因为这种存储需要的空间是固定的，在程序中这种uniform 的数量是受限的
            // 此处GetProgram是获取激活的uniform
            // 着色器的结构是要声明版本，接着是输入和输出变量、uniform和main函数
            // 其中的uniform就是一种给shader传递参数的重要方式
            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

            // Next, allocate the dictionary to hold the locations.
            _uniformLocations = new Dictionary<string, int>();
            // 取出所有uniform的位置，方便读取
            // Loop over all the uniforms,
            for (var i = 0; i < numberOfUniforms; i++)
            {
                // get the name of this uniform,
                var key = GL.GetActiveUniform(Handle, i, out _, out _);

                // get the location,
                var location = GL.GetUniformLocation(Handle, key);

                // and then add it to the dictionary.
                _uniformLocations.Add(key, location);
            }
        }

        /// <summary>
        /// 编译着色器，检查编译成果
        /// </summary>
        /// <param name="shader"></param>
        private static void CompileShader(int shader)
        {
            // Try to compile the shader
            GL.CompileShader(shader);

            // Check for compilation errors
            GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);
            if (code != (int)All.True)
            {
                // We can use `GL.GetShaderInfoLog(shader)` to get information about the error.
                var infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"Error occurred whilst compiling Shader({shader}).\n\n{infoLog}");
            }
        }
        /// <summary>
        /// 链接程序内的着色器
        /// </summary>
        /// <param name="program"></param>
        private static void LinkProgram(int program)
        {
            // We link the program
            GL.LinkProgram(program);

            // Check for linking errors
            // 检查是否错误
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);
            if (code != (int)All.True)
            {
                // We can use `GL.GetProgramInfoLog(program)` to get information about the error.
                throw new Exception($"Error occurred whilst linking Program({program})");
            }
        }

        // A wrapper function that enables the shader program.
        public void Use()
        {
            GL.UseProgram(Handle);
        }

        // The shader sources provided with this project use hardcoded layout(location)-s. If you want to do it dynamically,
        // you can omit the layout(location=X) lines in the vertex shader, and use this in VertexAttribPointer instead of the hardcoded values.
        // 可以动态获取着色器代码内设置的location，不使用硬编码的方式
        public int GetAttribLocation(string attribName)
        {
            return GL.GetAttribLocation(Handle, attribName);
        }

        // Uniform setters
        // Uniforms are variables that can be set by user code, instead of reading them from the VBO.
        // You use VBOs for vertex-related data, and uniforms for almost everything else.

        // Setting a uniform is almost always the exact same, so I'll explain it here once, instead of in every method:
        //     1. Bind the program you want to set the uniform on
        //     2. Get a handle to the location of the uniform with GL.GetUniformLocation.
        //     3. Use the appropriate GL.Uniform* function to set the uniform.

        /// <summary>
        /// Set a uniform int on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void SetInt(string name, int data)
        {
            GL.UseProgram(Handle);
            GL.Uniform1(_uniformLocations[name], data);//结尾数字代表参数数量
        }

        /// <summary>
        /// Set a uniform float on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void SetFloat(string name, float data)
        {
            GL.UseProgram(Handle);
            GL.Uniform1(_uniformLocations[name], data);
        }

        /// <summary>
        /// Set a uniform Matrix4 on this shader 
        /// 表示包含三维旋转、缩放、变换和投影的4x4矩阵。
        /// 
        /// 第一个参数你现在应该很熟悉了，它是uniform的位置值。
        /// 第二个参数告诉OpenGL我们将要发送多少个矩阵，这里是1。
        /// 第三个参数询问我们是否希望对我们的矩阵进行转置(Transpose)，也就是说交换我们矩阵的行和列。
        /// OpenGL开发者通常使用一种内部矩阵布局，叫做列主序(Column-major Ordering)布局。
        /// GLM的默认布局就是列主序，所以并不需要转置矩阵，我们填GL_FALSE。
        /// 最后一个参数是真正的矩阵数据，但是GLM并不是把它们的矩阵储存为OpenGL所希望接受的那种，因此我们要先用GLM的自带的函数value_ptr来变换这些数据。
        /// //https://learnopengl.com/code_viewer.php?code=in-practice/text_rendering
        /// glUniformMatrix4fv(transformLoc, 1, GL_FALSE, glm::value_ptr(trans));
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        /// <remarks>
        ///   <para>
        ///   The matrix is transposed before being sent to the shader.
        ///   </para>
        /// </remarks>
        public void SetMatrix4(string name, Matrix4 data, bool transpose = false)
        {
            GL.UseProgram(Handle);
            GL.UniformMatrix4(_uniformLocations[name], transpose, ref data);
        }

        /// <summary>
        /// Set a uniform Vector3 on this shader. 
        /// 3维矢量
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void SetVector3(string name, Vector3 data)
        {
            GL.UseProgram(Handle);
            GL.Uniform3(_uniformLocations[name], data);
        }

        /// <summary>
        /// 删除该着色器资源
        /// </summary>
        public void Remove() {
            GL.DeleteProgram(Handle);
        }

        /// <summary>
        /// 清理OpenGL状态机中的程序绑定
        /// </summary>
        public static void Clear() {
            GL.UseProgram(0);
        }

        /// <summary>
        /// 不需要远近的正射投影
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="bottom"></param>
        /// <param name="top"></param>
        /// <returns></returns>
        public static Matrix4 GLMOrthographic(float left, float right, float bottom, float top)//glm::ortho
        {
            var result = Matrix4.Identity;
            result.Row0.X = 2 / (right - left);
            result.Row1.Y = 2 / (top - bottom);
            result.Row2.Z = 1;
            result.Row3.X = -(right + left) / (right - left);
            result.Row3.Y = -(top + bottom) / (top - bottom);
            return result;
        }
    }
}
