using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GlacierLocalizationTools.Model
{
    static class LZ4Handler
    {
        [DllImport("lz4.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LZ4_compress(byte[] source, byte[] dest, int sourceSize);

        [DllImport("lz4.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LZ4_decompress_safe(byte[] source, byte[] dest, int compressedSize, int maxDecompressedSize);
    }
}
