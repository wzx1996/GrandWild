using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace org.flamerat.GrandWild.Resource {
    public class Image:IResourceLoader {
        public Image(string fileName,bool flipVertical=true) {
            var bitmap = new Bitmap(fileName);
            var lockedBitmap = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Width = (uint)bitmap.Width;
            Height = (uint)bitmap.Height;
            Data = new byte[bitmap.Height, bitmap.Width, 4];
            unsafe
            {
                //use UInt32* to copy word wise rather than byte wise
                UInt32* pSourceImage = (UInt32*)lockedBitmap.Scan0;
                fixed(byte* pData = Data) {
                    UInt32* pData32 = (UInt32*)pData;
                    if (flipVertical) {
                        for (uint i = 0; i <= lockedBitmap.Height; i++) {
                            for (uint j = 0; j <= lockedBitmap.Width; j++) {
                                UInt32 currentPixel = pSourceImage[i * lockedBitmap.Stride + j];
                                currentPixel = (currentPixel >> 24) + (currentPixel << 8); //ARGB -> RGBA
                                pData32[(i-lockedBitmap.Height) * lockedBitmap.Width + j] = currentPixel;
                            }
                        }
                    }else {
                        for (uint i = 0; i <= lockedBitmap.Height; i++) {
                            for (uint j = 0; j <= lockedBitmap.Width; j++) {
                                UInt32 currentPixel = pSourceImage[i * lockedBitmap.Stride + j];
                                currentPixel = (currentPixel >> 24) + (currentPixel << 8); //ARGB -> RGBA
                                pData32[i * lockedBitmap.Width + j] = currentPixel;
                            }
                        }
                    }
                    
                }
            }
            bitmap.UnlockBits(lockedBitmap);
            bitmap.Dispose();
        }
        public byte[,,/* RGBA */] Data { get; private set; }
        public uint Width { get; private set; }
        public uint Height { get; private set; }
    }
}
