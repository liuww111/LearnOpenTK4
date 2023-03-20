using FreeTypeSharp.Native;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using static FreeTypeSharp.Native.FT;

namespace LearnOpenTK.Common
{
    public class FontManage {
        private FontRenderer _fontRenderer;
        private FreeType _freeType;

        private string _stringFont;
        public FontManage(float widthScreen,
            float heightScreen,string fontFile,string wordFile) {
            if (File.Exists(fontFile)==false) {
                Debug.WriteLine("字型文件不存在");
                return;
            }
            if (File.Exists(wordFile) == false)
            {
                Debug.WriteLine("字库文件不存在");
                return;
            }
            _fontRenderer = new FontRenderer(widthScreen, heightScreen);
            _freeType = new FreeType(fontFile);
            _stringFont = File.ReadAllText(wordFile).Replace("\n", string.Empty).Replace("\r", string.Empty);
            //加载需要的字库
            foreach (var c in _stringFont)
            {
                new Character(c.ToString());
            }
            _fontRenderer.InitalizeGL();
            //使用freetype提前字形,对字形进行纹理渲染
            foreach (var ch in Character.Characters)
            {
                ch.Value.InitializeGL();
            }
            _freeType.Done();
        }

        public void PrintText(string text, float x, float y, float scale, Vector3 color) {
            _fontRenderer.PrintText(text,x,y,scale,color);
        }
    }

    public class FontRenderer
    {
        private int _vao;
        private int _vbo;

        private Shader _shader;

        private Matrix4 _projection;

        private float _width;
        private float _height;

        public FontRenderer(float widthScreen,
            float heightScreen)
        {
            _width = widthScreen;
            _height = heightScreen;
            //_projection = Matrix4.CreateOrthographic(_width, _height, -10f, 10f);//不需要远近
            //_projection = Matrix4.CreateOrthographic(_width, _height, 0.0f, 0.0f);
            //_projection = Matrix4.CreateOrthographicOffCenter(-_width / 2, _width / 2, -_height / 2, _height / 2, 0.0f, 0.0f);//功能同上

            _projection = Shader.GLMOrthographic(0.0f, _width, 0.0f, _height);
        }

        public void UpdateProjection(float widthScreen, float heightScreen)
        {
            _width = widthScreen;
            _height = heightScreen;
            //_projection = Matrix4.CreateOrthographic(_width, _height, 0.0f, 0.0f);
            _projection = Shader.GLMOrthographic(0.0f, _width, 0.0f, _height);
            //_projection = Matrix4.CreateOrthographic(_width, _height, -10f, 10f);
        }

        public void InitalizeGL()
        {
            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * 6 * 4, IntPtr.Zero, BufferUsageHint.DynamicDraw);

            _shader = new Shader("./Shaders/textShader.vert", "./Shaders/textShader.frag");
        }
        /// <summary>
        /// 显示文本
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="x">位置x</param>
        /// <param name="y">位置y</param>
        /// <param name="scale">比例</param>
        /// <param name="color">颜色</param>
        public void PrintText(string text, float x, float y, float scale, Vector3 color)
        {
            _shader.Use();
            //_shader.SetMatrix4("projection", _projection);
            _shader.SetMatrix4("projection", _projection);
            _shader.SetVector3("textColor", color);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindVertexArray(_vao);

            //x = _width * (x - 0.5f);
            //y = (_height - 48 * 2) * (-y + 0.5f);
            
            //右上角为(0,0)原点
            //x = x - _width / 2;
            //y = _height / 2 - y;

            for (int i = 0; i < text.Length; i++)
            {
                Character c;
                if (Character.Characters.TryGetValue(text[i].ToString(), out Character value))
                {
                    c = value;
                }
                else
                {
                    c = Character.Characters["?"];//异常字符
                }

                float xpos = x + c.Bearing.X * scale;
                float ypos = y - (c.Size.Y - c.Bearing.Y) * scale;

                float w = c.Size.X * scale;
                float h = c.Size.Y * scale;

                float[] vertices = new float[]
                {
                    xpos, ypos+h, 0.0f, 0.0f,
                    xpos, ypos, 0.0f, 1.0f,
                    xpos + w, ypos, 1.0f, 1.0f,

                    xpos, ypos+h, 0.0f, 0.0f,
                    xpos + w, ypos, 1.0f, 1.0f,
                    xpos + w, ypos+h, 1.0f, 0.0f
                };

                GL.BindTexture(TextureTarget.Texture2D, c.Texture);

                GL.EnableVertexAttribArray(0);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
                GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 0, 0);

                GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicDraw);

                GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

                x += (c.Advance >> 6) * scale;//Advance（单位：1/64像素）2的6次幂是64
            }
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }
    }

    public class Character
    {
        public static Dictionary<string, Character> Characters = new Dictionary<string, Character>();

        private string _char;

        private int _textureGl;

        private Vector2i _size;
        private Vector2i _bearing;

        private int _advance;

        public int Texture { get { return _textureGl; } }
        /// <summary>
        /// 大小宽高
        /// </summary>
        public Vector2i Size { get { return _size; } }
        /// <summary>
        /// X水平距离 Y垂直距离
        /// </summary>
        public Vector2i Bearing { get { return _bearing; } }
        public int Advance { get { return _advance; } }

        public Character(string Char)
        {
            _char = Char;

            if (!Characters.ContainsKey(_char))
            {
                Characters[_char] = this;
            }
        }

        public void InitializeGL()
        {
            var freetype = FreeType.instance;

            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
            freetype.LoadChar(_char);
            var bitmap = freetype.GetBitmap();

            _size = new Vector2i((int)bitmap.width, (int)bitmap.rows);
            _bearing = freetype.GetBearing();
            _advance = freetype.GetAdvance().X;

            _textureGl = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _textureGl);
            GL.TexImage2D(TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rgb,
                (int)bitmap.width,
                (int)bitmap.rows,
                0,
                PixelFormat.Red,
                PixelType.UnsignedByte,
                bitmap.buffer
                );

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);


        }
    }

    public class FreeType
    {
        private IntPtr _library;
        private IntPtr _face;

        private unsafe FT_FaceRec* _faceRec;

        public static FreeType instance;

        public unsafe FreeType(string path,uint fontSize=48)
        {
            if (instance == null) instance = this;

            FT_Error error = FT.FT_Init_FreeType(out _library);
            if (error != FT_Error.FT_Err_Ok)
            {
                Debug.WriteLine("ERROR:初始化FreeType 失败");
                return;
            }

            FT.FT_Library_Version(_library, out var major, out var minor, out var patch);
            Debug.WriteLine($"FreeType Version major:{major} minor:{minor} patch:{patch}");

            error = FT.FT_New_Face(_library, path, 0, out _face);
            if (error != FT_Error.FT_Err_Ok)
            {
                Debug.WriteLine("ERROR:初始化字体失败");
                return;
            }

            this.SetPixelSize(0, fontSize);

            _faceRec = (FT_FaceRec*)(void*)_face;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Done()
        {
            FT.FT_Done_Face(_face);
            FT.FT_Done_FreeType(_library);
        }

        public FT_Bitmap GetBitmap()
        {
            FT_Bitmap result;
            unsafe
            {
                result = _faceRec->glyph->bitmap;
            }
            return result;
        }
        /// <summary>
        /// 水平距离 bearingX
        /// 垂直距离 bearingY
        /// </summary>
        /// <returns></returns>
        public Vector2i GetBearing()
        {
            int x;
            int y;

            unsafe
            {
                x = _faceRec->glyph->bitmap_left;
                y = _faceRec->glyph->bitmap_top;
            }

            return new Vector2i(x, y);
        }
        /// <summary>
        /// 水平预留值
        /// </summary>
        /// <returns></returns>
        public Vector2i GetAdvance()
        {
            uint x;
            uint y;

            unsafe
            {
                x = (uint)_faceRec->glyph->advance.x;
                y = (uint)_faceRec->glyph->advance.y;
            }

            return new Vector2i((int)x, (int)y);
        }
        /// <summary>
        /// 字体大小，将宽度值设为0表示我们要从字体面通过给定的高度中动态计算出字形的宽度
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void SetPixelSize(uint width, uint height)
        {
            FT.FT_Set_Pixel_Sizes(_face, width, height);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Char"></param>
        public void LoadChar(string Char)
        {
            uint code = (uint)char.ConvertToUtf32(Char, 0);
            var index = FT.FT_Get_Char_Index(_face, code);

            FT.FT_Load_Glyph(_face, index, FT_LOAD_RENDER);
        }
    }
}
