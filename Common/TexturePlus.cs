﻿using OpenTK.Graphics.OpenGL4;
using System.Drawing;
using System.Drawing.Imaging;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;
using StbImageSharp;
using System.IO;
using System.Diagnostics;

namespace LearnOpenTK.Common
{
    // 加载纹理贴图的帮助类
    public class TexturePlus
    {
        public readonly int Handle;

        public readonly int Width;
        public readonly int Height;

        public int X;
        public int Y;

        private int _elementBufferObject;

        private int _vertexBufferObject;

        private int _vertexArrayObject;

        public static TexturePlus LoadFromFile(string path)
        {
            int handle = GL.GenTexture();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, handle);
            StbImage.stbi_set_flip_vertically_on_load(1);
            int width = 0,height=0;
            using (Stream stream = File.OpenRead(path))
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
            return new TexturePlus(handle, width ,height);
        }

        public TexturePlus(int glHandle,int width, int height, int x = 0, int y = 0)
        {
            Handle = glHandle;
            Width = width;
            Height = height;
            Init(x, y);
        }

        private readonly uint[] _indices =
        {
            0, 1, 3,
            1, 2, 3
        };
        public uint[] Indices { get { return _indices; } }

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
        /// 所在位置
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

        public void Use(TextureUnit unit = TextureUnit.Texture0)
        {
            GL.BindVertexArray(_vertexArrayObject);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);

            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, Handle);
            GL.DrawElements(PrimitiveType.Triangles, this.Indices.Length, DrawElementsType.UnsignedInt, 0);
        }

        public void MovePosition(int x = 0, int y = 0) {
            var vertices = getVertices(x, y);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferSubData(BufferTarget.ArrayBuffer, System.IntPtr.Zero, sizeof(float) * vertices.Length, vertices);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        /// <summary>
        /// 删除纹理
        /// </summary>
        public void Remove()
        {
            GL.DeleteTexture(Handle);
        }
    }
}
