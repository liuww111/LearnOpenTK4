using LearnOpenTK.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using System.Collections.Generic;
using FreeTypeSharp;
using System;
using FreeTypeSharp.Native;
using System.Diagnostics;

namespace LearnOpenTK
{
    public class Window : GameWindow
    {
        // Because we're adding a texture, we modify the vertex array to include texture coordinates.
        // Texture coordinates range from 0.0 to 1.0, with (0.0, 0.0) representing the bottom left, and (1.0, 1.0) representing the top right.
        // The new layout is three floats to create a vertex, then two floats to create the coordinates.
        // texture纹理的使用
        private readonly float[] _vertices =
        {
            // Position         Texture coordinates
             0.5f,  0.5f, 0.0f, 1.0f, 1.0f, // top right
             0.5f, -0.5f, 0.0f, 1.0f, 0.0f, // bottom right
            -0.5f, -0.5f, 0.0f, 0.0f, 0.0f, // bottom left
            -0.5f,  0.5f, 0.0f, 0.0f, 1.0f  // top left
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

        // For documentation on this, check Texture.cs.
        private Texture _texture;

        internal struct Character
        {
            public int TextureID;  // 纹理ID
            public Vector2i Size;       // Size of glyph
            public Vector2i Bearing;    // Offset from baseline to left/top of glyph
            public int Advance;    // Offset to advance to next glyph
        };
        readonly Dictionary<char, Character> Characters = new Dictionary<char, Character>();

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            LoadFont();
            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            // The shaders have been modified to include the texture coordinates, check them out after finishing the OnLoad function.
            // 注意sampler2D是纹理的一个类型，默认是使用0号纹理单元
            // 每个采样器与每个纹理关联
            // 给采样器分配一个位置值（默认纹理单元是0，所以多个纹理单元需要多个分配）
            // OpenGL至少保证有16个纹理单元供你使用(对着色器的操作)
            _shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");
            _shader.Use();


            // Because there's now 5 floats between the start of the first vertex and the start of the second,
            // we modify the stride from 3 * sizeof(float) to 5 * sizeof(float).
            // This will now pass the new vertex array to the buffer.
            // 动态的从着色器内获取location
            // 步进是5
            var vertexLocation = _shader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

            // Next, we also setup texture coordinates. It works in much the same way.
            // We add an offset of 3, since the texture coordinates comes after the position data.
            // We also change the amount of data to 2 because there's only 2 floats for texture coordinates.
            // 纹理坐标是两个float
            var texCoordLocation = _shader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

            _texture = Texture.LoadFromFile("Resources/Tile.png");
            _texture.Use(TextureUnit.Texture0);
            //在着色器内通过使用uniform得到了texture，每个采样器都绑定一个纹理单元
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.BindVertexArray(_vertexArrayObject);

            _texture.Use(TextureUnit.Texture0);
            //GL.BindTexture(TextureTarget.Texture2D, Characters['0'].TextureID);
            _shader.Use();
            //RenderText("T", 25.0f, 25.0f, 1.0f, new Vector3(0.5f, 0.8f, 0.2f));
            //RenderText("2", 540.0f, 570.0f, 0.5f, new Vector3(0.3f, 0.7f, 0.9f));

            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);

            SwapBuffers();
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

            GL.Viewport(0, 0, Size.X, Size.Y);
        }

        protected void LoadFont() {
            #region 加载字体
            //参考：https://learnopengl.com/code_viewer.php?code=in-practice/text_rendering
            var library = new FreeTypeLibrary();
            IntPtr ft = library.Native;
            IntPtr internalface = IntPtr.Zero;
            //c查看版本
            FT.FT_Library_Version(library.Native, out var major, out var minor, out var patch);
            Debug.WriteLine($"major:{major} minor:{minor} patch:{patch}");
            //初始化Freetype
            FT_Error error = FT.FT_Init_FreeType(out ft);
            if (error != FT_Error.FT_Err_Ok)
            {
                Debug.WriteLine("ERROR:初始化FreeType 失败");
                return;
            }
            //初始化字体
            error = FT.FT_New_Face(ft, @"D:\workspaces\LearnOpenTK\font\arial2.ttf", 0, out internalface);
            if (error != FT_Error.FT_Err_Ok)
            {
                Debug.WriteLine("ERROR:初始化字体失败");
                return;
            }
            var face = new FreeTypeFaceFacade(library, internalface);
            //定义字体大小
            error = FT.FT_Set_Pixel_Sizes(face.Face, 0, 48);
            FT.FT_Set_Pixel_Sizes(face.Face, 0, 48);
            //glPixelStorei(GL_UNPACK_ALIGNMENT, 1); 
            //禁用字节对齐限制
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
            //GL.ActiveTexture(TextureUnit.Texture0);
            for (uint c = 0; c < 128; c++)
            {
                error = FT.FT_Load_Char(face.Face, c, FT.FT_LOAD_RENDER);
                if (error != FT_Error.FT_Err_Ok)
                {
                    Debug.WriteLine("ERROR:加载字型失败");
                    return;
                }
                int texture = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, texture);
                GL.TexImage2D(
                    TextureTarget.Texture2D,
                    0,
                    PixelInternalFormat.CompressedRed,
                    (int)face.GlyphBitmap.width,
                    (int)face.GlyphBitmap.rows,
                    0,
                    PixelFormat.Red,
                    PixelType.UnsignedByte,
                    face.GlyphBitmap.buffer);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                Character character = new Character()
                {
                    TextureID = texture,
                    Size = new Vector2i((int)face.GlyphBitmap.width, (int)face.GlyphBitmap.rows),
                    Bearing = new Vector2i(face.GlyphBitmapLeft, face.GlyphBitmapTop),
                    Advance = face.GlyphMetricHorizontalAdvance
                };
                Characters.Add((char)c, character);
            }
            GL.BindTexture(TextureTarget.Texture2D, 0);
            FT.FT_Done_Face(face.Face);
            FT.FT_Done_FreeType(ft);
            #endregion
        }

        protected void RenderText(string text, float x, float y, float scale, Vector3 color)
        {
            _shader.Use();
            GL.Uniform3(GL.GetUniformLocation(_shader.Handle, "textColor"), color.X, color.Y, color.Z);
            GL.BindVertexArray(_vertexArrayObject);
            GL.ActiveTexture(TextureUnit.Texture0);

            foreach (char c in text)
            {
                if (Characters.ContainsKey(c))
                {
                    Character ch = Characters[c];
                    float xpos = x + ch.Bearing.X * scale;
                    float ypos = y - (ch.Size.Y - ch.Bearing.Y) * scale;
                    float w = ch.Size.X * scale;
                    float h = ch.Size.Y * scale;
                    // 对每个字符更新VBO
                    float[] vertices = new float[]{
                     xpos,     ypos + h,   0.0f, 0.0f ,
                     xpos,     ypos,       0.0f, 1.0f ,
                     xpos + w, ypos,       1.0f, 1.0f ,

                     xpos,     ypos + h,   0.0f, 0.0f ,
                     xpos + w, ypos,       1.0f, 1.0f ,
                     xpos + w, ypos + h,   1.0f, 0.0f
                };
                    //在四边形上绘制字形纹理
                    GL.BindTexture(TextureTarget.Texture2D, ch.TextureID);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
                    GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, sizeof(float) * vertices.Length, vertices);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                    GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
                    //x += (ch.Advance >> 6) * scale;
                    //x += ch.Advance * x;
                }
            }
            GL.BindVertexArray(0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

    }
}