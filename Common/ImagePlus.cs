using OpenTK.Graphics.OpenGL4;
using System.Drawing;
using System.Drawing.Imaging;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;
using StbImageSharp;
using System.IO;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using OpenTK.Mathematics;

namespace LearnOpenTK.Common
{
    /// <summary>
    /// 加载纹理贴图的帮助类
    /// </summary>
    public class ImagePlus
    {
        private int TextureHandle;

        private Shader _shader;

        private int _elementBufferObject;

        private int _vertexBufferObject;

        private int _vertexArrayObject;

        private readonly uint[] _indices =
        {
            0, 1, 3,
            1, 2, 3
        };
        //索引
        public uint[] Indices { get { return _indices; } }
        //图像宽高
        public readonly int Width;
        public readonly int Height;

        //位置
        public int X;
        public int Y;

        /// <summary>
        /// 图片和对应的着色器
        /// </summary>
        /// <param name="imgPath"></param>
        /// <param name="shader"></param>
        /// <returns></returns>
        public static ImagePlus Load(string imgPath, Shader shader)
        {
            if (File.Exists(imgPath) == false)
            {
                throw new Exception($"地址：imgPath {imgPath}，不存在");
            }
            int handle = GL.GenTexture();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, handle);
            StbImage.stbi_set_flip_vertically_on_load(1);
            int width = 0,height=0;
            using (Stream stream = File.OpenRead(imgPath))
            {
                ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
                width = image.Width;
                height = image.Height;
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
            }
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            //
            return new ImagePlus(shader, handle, width, height);
        }

        public ImagePlus(Shader shader, int glHandle,int width, int height, int x = 0, int y = 0)
        {
            TextureHandle = glHandle;
            Width = width;
            Height = height;
            Init(x, y);
            _shader = shader;
        }

        private float[] getVertices(int x, int y) {
            X = x;
            Y = y;
            float[] vertices = {
                Width+x, Height+y,  1.0f, 1.0f, // top right
                Width+x, 0.0f+y, 1.0f, 0.0f, // bottom right
                x, y, 0.0f, 0.0f, // bottom left
                0.0f+x,  Height+y, 0.0f, 1.0f  // top left
            };
            return vertices;
        }

        /// <summary>
        /// 初始所在位置
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void Init(int x = 0, int y = 0) {
            float[] vertices = getVertices(x,y);
            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            //GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            //改成动态
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicDraw);

            _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        private void TextureUse(TextureUnit unit = TextureUnit.Texture0)
        {
            GL.BindVertexArray(_vertexArrayObject);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);

            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, TextureHandle);
            GL.DrawElements(PrimitiveType.Triangles, this.Indices.Length, DrawElementsType.UnsignedInt, 0);
        }

        /// <summary>
        /// 开始使用
        /// </summary>
        /// <param name="unit"></param>
        public void Use(TextureUnit unit = TextureUnit.Texture0)
        {
            ImagePlus.Clear();
            _shader.Use();
            this.TextureUse(unit);
        }

        /// <summary>
        /// 改变图像位置
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void MovePosition(int x = 0, int y = 0) {
            var vertices = getVertices(x, y);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferSubData(BufferTarget.ArrayBuffer, System.IntPtr.Zero, sizeof(float) * vertices.Length, vertices);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Remove()
        {
            _shader.Remove();
            GL.DeleteTexture(TextureHandle);
        }

        /// <summary>
        /// 清理OpenGL状态机中的着色器和纹理的绑定
        /// </summary>
        public static void Clear()
        {
            //清除Shader绑定
            GL.UseProgram(0);
            //清除纹理绑定
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

    }
}
