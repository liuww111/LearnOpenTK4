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
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using System.IO;
using System.Threading;
using LearnOpenTK.Common;

namespace LearnOpenTK
{

    public class Window : GameWindow
    {
        private float frameTime = 0.0f;
        private int fps = 0;

        private ALDevice device;
        private ALContext context;

        private readonly AudioContext _context;
        private readonly int _buffer=0;
        private readonly int _source=0;

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

            //https://github.com/naudio/NAudio

 
            string filename = @"D:\workspaces\LearnOpenTK\LearnOpenTK\dll\且听风呤.wav";
            var soundFile = new WavLoader(File.Open(filename, FileMode.Open));

            _buffer = AL.GenBuffer();
            AL.BufferData(_buffer, soundFile.Format, soundFile.Data, soundFile.Data.Length, soundFile.Rate);

            _source = AL.GenSource();
            AL.Source(_source, ALSourcei.Buffer, _buffer);
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
        private void InitAudio()
        {
            int[] attr = { 0 };
            string defname = ALC.GetString(ALDevice.Null, AlcGetString.DefaultDeviceSpecifier);
            device = ALC.OpenDevice(defname);
            context = ALC.CreateContext(device, attr);
            ALC.MakeContextCurrent(context);
            ALC.ProcessContext(context);
            AL.Listener(ALListenerf.Gain, 0.5f);
        }

        private void Clean()
        {
            AL.SourceStop(source);
            AL.DeleteSource(source);
            AL.DeleteBuffer(buffer);
        }
         

        public float[] LoadWav(string filename, out int _channels, out int _bitsPerSample, out int _sampleRate)
        {
            using (BinaryReader reader = new BinaryReader(File.Open(filename, FileMode.Open)))
            {
                Stream baseStream = reader.BaseStream;

                string signature = new string(reader.ReadChars(4));
                Console.WriteLine(signature);
                if (signature != "RIFF")
                    throw new NotSupportedException("Not wav format.");

                int riff_chunck_size = reader.ReadInt32();

                string format = new string(reader.ReadChars(4));
                if (format != "WAVE")
                    throw new NotSupportedException("Not wav format.");

                string format_signature = "";
                byte[] data = null!;
                _channels = 0;
                _bitsPerSample = 0;
                _sampleRate = 0;

                while (baseStream.Position != baseStream.Length)
                {
                    format_signature = new string(reader.ReadChars(4));
                    if (format_signature == "fmt ")
                    {
                        reader.ReadInt32(); //chunk size
                        int fmt = reader.ReadInt16(); //audio format
                        int num_channels = reader.ReadInt16(); //number of channels
                        int sample_rate = reader.ReadInt32(); //sample rate
                        reader.ReadInt32(); //byte rate
                        reader.ReadInt16(); //block algin
                        int bits_per_sample = reader.ReadInt16(); //bits per sample
                        if (fmt != 0x01)
                        {
                            throw new NotSupportedException("unsupport format. PCM format only");
                        }
                        _channels = num_channels;
                        _bitsPerSample = bits_per_sample;
                        _sampleRate = sample_rate;
                    }
                    else if (format_signature == "data")
                    {
                        int data_chunk_size = reader.ReadInt32();
                        data = reader.ReadBytes(data_chunk_size);
                    }
                    else
                    {
                        int chunk_size = reader.ReadInt32();
                        reader.ReadBytes(chunk_size);
                    }
                }
                if (_bitsPerSample == 8)
                {
                    return Convert8ToFloat32(data);
                }
                else if (_bitsPerSample == 16)
                {
                    return Convert16ToFloat32(data);
                }
                else
                {
                    return ConvertFloat32(data);
                }
            }
        }

        private float[] ConvertFloat32(byte[] data)
        {
            int data_index = 0;
            int result_size = 0;
            int result_index = 0;

            result_size = (int)((data.Length / 3));
            float[] result_data = new float[result_size];

            while (data_index < data.Length)
            {
                byte c1 = data[data_index++];
                byte c2 = data[data_index++];
                byte c3 = data[data_index++];

                int value = (int)(((c3 & 0x000000ff) << 16) | ((c2 & 0x000000ff) << 8) | (c1 & 0x000000ff));
                value = (int)((value & 0x00ffffff) | (((value & 0x800000) != 0) ? 0xff000000 : 0x00000000));

                result_data[result_index++] = (float)value / 8388607.0f;
            }
            return result_data;
        }

        private float[] Convert16ToFloat32(byte[] data)
        {
            int data_index = 0;
            int result_size = 0;
            int result_index = 0;

            result_size = (int)((data.Length / 2));
            float[] result_data = new float[result_size];

            while (data_index < data.Length)
            {
                byte c1 = data[data_index++];
                byte c2 = data[data_index++];

                int value = (int)(((c2 & 0x000000ff) << 8) | (c1 & 0x000000ff));
                value = (int)((value & 0x0000ffff) | (((value & 0x8000) != 0) ? 0xffff0000 : 0x00000000));

                result_data[result_index++] = (float)value / 65535.0f;
            }
            return result_data;
        }

        private float[] Convert8ToFloat32(byte[] data)
        {
            int data_index = 0;
            int result_size = 0;
            int result_index = 0;

            result_size = data.Length;
            float[] result_data = new float[result_size];

            while (data_index < data.Length)
            {
                byte c1 = data[data_index++];

                int value = (int)c1 - 128;
                result_data[result_index++] = (float)value / 128;
            }
            return result_data;
        }

        private void Play(float[] stream, int sampleRate, int channels)
        {
            buffer = AL.GenBuffer();
            source = AL.GenSource();

            ALFormat fmt = ALFormat.StereoFloat32Ext;
            if (channels == 1)
            {
                fmt = ALFormat.MonoFloat32Ext;
            }

            AL.BufferData(buffer, fmt, stream, sampleRate);
            AL.Source(source, ALSourcei.Buffer, buffer);
            AL.SourcePlay(source);
        }

        private void Wait()
        {
            int state;
            do
            {
                Thread.Sleep(1000);
                Console.Write(".");
                AL.GetSource(source, ALGetSourcei.SourceState, out state);
            } while ((ALSourceState)state == ALSourceState.Playing);
            Console.WriteLine("finished playback");
        }
    }
}
