using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;

namespace LearnOpenTK
{
    public static class Program
    {
        private static void Main()
        {
            var nativeWindowSettings = new NativeWindowSettings()
            {
                Size = new Vector2i(800, 600),
                Flags = ContextFlags.ForwardCompatible,
                APIVersion = new Version(4, 6),
                Profile = ContextProfile.Core,//可以开启兼容模式
                API = ContextAPI.OpenGL,
                WindowBorder = WindowBorder.Resizable,
                WindowState = WindowState.Normal //Fullscreen可以全屏
            };

            // To create a new window, create a class that extends GameWindow, then call Run() on it.
            //创建窗口需要扩展GameWindow的类，然后对其调用Run
            using (var window = new Window(GameWindowSettings.Default, nativeWindowSettings))
            {
                window.Run();
            }

            // And that's it! That's all it takes to create a window with OpenTK.
        }
    }
}
