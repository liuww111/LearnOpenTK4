using System;
using System.Runtime.InteropServices;

//#if WINDOWS
//using CLong = System.Int32;
//#elif LINUX
//using CLong = System.Int64;
//#endif
//参考
//https://www.cnblogs.com/gamedevboy/p/5634665.html
//https://freetype.org/freetype2/docs/reference/ft2-version.html

namespace FT {
    using CLong = System.Int32;

    #region struct 
    [StructLayout(LayoutKind.Sequential)]
    public struct FT_BBox
    {
        public CLong XMin;
        public CLong YMin;
        public CLong XMax;
        public CLong YMax;
    }
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct FT_Bitmap
    {
        public uint Rows;
        public uint Width;
        public int Pitch;
        public byte* Buffer;
        public ushort NumGrays;
        public byte PixelMode;
        public byte PaletteMode;
        public void* Palette;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct FT_Bitmap_Size
    {
        public short Height;
        public short Width;

        public CLong Size;

        public CLong Xppem;
        public CLong Yppem;
    }
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct FT_Charmap
    {
        public FT_Face* Face;
        public FT_Encoding Encoding;
        public ushort PlatformID;
        public ushort EncodingID;
    }
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct FT_Face
    {
        public CLong NumFaces;
        public CLong FaceIndex;

        public CLong FaceFlags;
        public CLong StyleFlags;

        public CLong NumGlyphs;

        public char* FamilyName;
        public char* StyleName;

        public int NumFixedSizes;
        public FT_Bitmap_Size* AvailableSizes;

        public int NumCharmaps;
        public FT_Charmap** Charmaps;

        public FT_Generic Generic;

        public FT_BBox BBox;

        public ushort UnitsPerEM;
        public short Ascender;
        public short Descender;
        public short Height;

        public short MaxAdvanceWidth;
        public short MaxAdvanceHeight;

        public short UnderlinePosition;
        public short UnderlineThickness;

        public FT_GlyphSlot* Glyph;
        public FT_Size* Size;
        public FT_Charmap* Charmap;

        public IntPtr Driver;
        public IntPtr Memory;
        public IntPtr Stream;

        public IntPtr SizesList;
        public FT_Generic AutoHint;
        public void* Extensions;
        public IntPtr Internal;
    }
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct FT_Generic
    {
        public void* Data;
        public void* Finalizer;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct FT_Glyph_Metrics
    {
        public CLong Width;
        public CLong Height;

        public CLong HoriBearingX;
        public CLong HoriBearingY;
        public CLong HoriAdvance;

        public CLong VertBearingX;
        public CLong VertBearingY;
        public CLong VertAdvance;
    }
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct FT_GlyphSlot
    {
        public IntPtr Library;
        public FT_Face* Face;
        public FT_GlyphSlot* Next;
        public uint GlyphIndex;
        public FT_Generic Generic;

        public FT_Glyph_Metrics Metrics;
        public CLong LinearHoriAdvance;
        public CLong LinearVertAdvance;
        public FT_Vector Advance;

        public FT_Glyph_Format Format;

        public FT_Bitmap Bitmap;
        public int BitmapLeft;
        public int BitmapTop;

        public FT_Outline Outline;

        public uint NumSubglyphs;
        public IntPtr Subglyphs;

        public void* ControlData;
        public CLong ControlLength;

        public CLong LSBDelta;
        public CLong RSBDelta;

        public void* Other;

        public IntPtr Internal;
    }
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct FT_Outline
    {
        public short NumContours;
        public short NumPoints;

        public FT_Vector* Points;
        public byte* Tags;
        public short* Contours;

        public int Flags;
    }
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct FT_Size
    {
        public FT_Face* Face;
        public FT_Generic Generic;
        public FT_Size_Metrics Metrics;
        public IntPtr Internal;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct FT_Size_Metrics
    {
        public ushort XPPem;
        public ushort YPPem;

        public CLong XScale;
        public CLong YScale;

        public CLong Ascender;
        public CLong Descender;
        public CLong Height;
        public CLong MaxAdvance;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct FT_Vector
    {
        private CLong _x;
        private CLong _y;

        public int X => (_x >> 6);
        public int Y => (_y >> 6);
    }
    #endregion

    #region enum
    public enum FT_Glyph_Format : uint
    {
        None,
        Composite = ('c' << 24) | ('o' << 16) | ('m' << 8) | 'p',
        Bitmap = ('b' << 24) | ('i' << 16) | ('t' << 8) | 's',
        Outline = ('o' << 24) | ('u' << 16) | ('t' << 8) | 'l',
        Plotter = ('p' << 24) | ('l' << 16) | ('o' << 8) | 't',
        SVG = ('S' << 24) | ('V' << 16) | ('G' << 8) | ' ',
    }
    public enum FT_Encoding : uint
    {
        None,
        MSSymbol = ('s' << 24) | ('y' << 16) | ('m' << 8) | 'b',
        Unicode = ('u' << 24) | ('n' << 16) | ('i' << 8) | 'c',
        SJIS = ('s' << 24) | ('j' << 16) | ('i' << 8) | 's',
        PRC = ('g' << 24) | ('b' << 16) | (' ' << 8) | ' ',
        Big5 = ('b' << 24) | ('i' << 16) | ('g' << 8) | '5',
        Wansung = ('w' << 24) | ('a' << 16) | ('n' << 8) | 's',
        Johab = ('j' << 24) | ('o' << 16) | ('h' << 8) | 'a',

        GB2312 = PRC,
        MSSJIS = SJIS,
        MSGB2312 = PRC,
        MSBig5 = Big5,
        MSWansung = Wansung,
        MSJohab = Johab,

        AdobeStandard = ('A' << 24) | ('D' << 16) | ('O' << 8) | 'B',
        AdobeExpert = ('A' << 24) | ('D' << 16) | ('B' << 8) | 'E',
        AdobeCustom = ('A' << 24) | ('D' << 16) | ('B' << 8) | 'C',
        AdobeLatin1 = ('l' << 24) | ('a' << 16) | ('t' << 8) | '1',

        OldLatin2 = ('l' << 24) | ('a' << 16) | ('t' << 8) | '2',
        AppleRoman = ('a' << 24) | ('r' << 16) | ('m' << 8) | 'n'
    }
    public enum FTLoad
    {
        Default,
        Render = 1 << 2
    }
    #endregion

    public static unsafe class FreeType
    {
        private const string LibraryName = "libfreetype";

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int FT_Init_FreeType(IntPtr* library);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int FT_Done_FreeType(IntPtr library);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int FT_New_Face(IntPtr library, string path, CLong index, FT_Face* face);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int FT_New_Memory_Face(IntPtr library, byte* file, CLong size, CLong index, FT_Face* face);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int FT_Done_Face(FT_Face* face);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int FT_Set_Pixel_Sizes(FT_Face* face, uint width, uint height);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int FT_Load_Char(FT_Face* face, uint c, FTLoad flags);
    }
}

