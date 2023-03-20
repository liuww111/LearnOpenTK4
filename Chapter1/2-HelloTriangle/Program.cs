using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace LearnOpenTK
{
    public static class Program
    {
        private static void Main()
        {
            var nativeWindowSettings = new NativeWindowSettings()
            {
                Size = new Vector2i(800, 600),
                Title = "LearnOpenTK - Creating a Window",
                // This is needed to run on macos
                Flags = ContextFlags.ForwardCompatible,
            };

            using (var window = new Window(GameWindowSettings.Default, nativeWindowSettings))
            {
                window.Run();
            }
            /*
             * 参考：
             * https://www.cnblogs.com/xiangqi/p/14608073.html
             * https://learnopengl-cn.github.io/
             概念补充：
            opengl是一个标准，每个图形卡都有各自的实现。调用api就是与显卡通信，我们需要给显卡需要处理的数据、
            告诉他数据的分布结构，在OpenGL里面任何二维图形都可以拆分成若干个三角形的,比如正方形是两个直角三角形
            OpenGL的坐标系为右手坐标系，即坐标x轴向右为正，y轴向上为正，z轴屏幕朝外为正，原点为为图像中心
             */
        }
    }
}
