﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ShimLib {
    public class Util {
        public static double GetPastTimeMs(long tickStart) {
            long tickEnd = Stopwatch.GetTimestamp();
            return (tickEnd - tickStart) * 1000.0 / Stopwatch.Frequency;
        }

        // 범위 제한 함수
        public static int IntClamp(int value, int min, int max) {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public unsafe static IntPtr memset(IntPtr _Dst, int _Val, long _Size) {
            byte valByte = (byte)_Val;
            byte* pdst = (byte*)_Dst.ToPointer();
            for (long i = 0; i < _Size; i++, pdst++) {
                *pdst = valByte;
            }
            return _Dst;
        }

        public unsafe static IntPtr memset4(IntPtr _Dst, uint _Val, long _Size) {
            uint* pdst = (uint*)_Dst.ToPointer();
            for (long i = 0; i < _Size; i++, pdst++) {
                *pdst = _Val;
            }
            return _Dst;
        }

        public unsafe static IntPtr memcpy(IntPtr _Dst, IntPtr _Src, long _Size) {
            byte* psrc = (byte*)_Src.ToPointer();
            byte* pdst = (byte*)_Dst.ToPointer();
            for (long i = 0; i < _Size; i++, psrc++, pdst++) {
                *pdst = *psrc;
            }
            return _Dst;
        }

        public unsafe static IntPtr memcpy4(IntPtr _Dst, IntPtr _Src, long _Size) {
            uint* psrc = (uint*)_Src.ToPointer();
            uint* pdst = (uint*)_Dst.ToPointer();
            for (long i = 0; i < _Size; i++, psrc++, pdst++) {
                *pdst = *psrc;
            }
            return _Dst;
        }

        // 8bit bmp 파일 버퍼에 로드
        public static bool Load8BitBmp(IntPtr buf, int bw, int bh, string filePath) {
            // 파일 오픈
            FileStream sr;
            try {
                sr = File.OpenRead(filePath);
            } catch {
                return false;
            }

            BinaryReader br = new BinaryReader(sr);
            sr.Position = 10;
            uint bfOffBits = br.ReadUInt32();
            sr.Position = 18;
            int biWidth = br.ReadInt32();
            sr.Position = 22;
            int biHeight = br.ReadInt32();
            sr.Position = 28;
            UInt16 biBitCount = br.ReadUInt16();
            if (biBitCount != 8) {  // 컬러비트 체크
                br.Dispose();
                sr.Dispose();
                return false;
            }

            sr.Position = bfOffBits;

            int fbw = biWidth;
            int fbh = biHeight;

            // bmp파일은 파일 저장시 라인당 4byte padding을 한다.
            // bw가 4로 나눠 떨어지지 않을경우 padding처리 해야 함
            // int stride = (bw+3)/4*4;
            int fstep = (fbw + 3) / 4 * 4;
    
            byte[] fbuf = br.ReadBytes(fbh * fstep);

            // 대상버퍼 width/height 소스버퍼 width/height 중 작은거 만큼 카피
            int minh = Math.Min(bh, fbh);
            int minw = Math.Min(bw, fbw);
                
            // bmp파일은 위아래가 뒤집혀 있으므로 파일에서 아래 라인부터 읽어서 버퍼에 쓴다
            for (int y = 0; y < minh; y++) {
                Marshal.Copy(fbuf, (fbh-y-1) * fstep, buf + y * bw, minw);
            }

            br.Dispose();
            sr.Dispose();
            return true;
        }

        int[] grayPalette = Enumerable.Range(0, 256).Select(i => (i | i << 8 | i << 16 | 0xff << 24)).ToArray();
        public static bool Save8BitBmp(IntPtr buf, int bw, int bh, string filePath) {
            //DWORD bytesWritten;
            //int bufSize = bw * bh;

            //// 파일 오픈
            //HANDLE hFile = CreateFile(filePath, GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE | FILE_SHARE_DELETE,
            //    NULL, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
            //if (hFile == INVALID_HANDLE_VALUE)   // 파일오픈 체크
            //    return FALSE;

            //// 파일 헤더
            //BITMAPFILEHEADER fh;
            //fh.bfType = 0x4d42;  // Magic NUMBER "BM"
            //fh.bfOffBits = sizeof(BITMAPFILEHEADER) + sizeof(BITMAPINFOHEADER) + sizeof(grayPalette);   // offset to bitmap buffer from start
            //fh.bfSize = fh.bfOffBits + bufSize;  // file size
            //fh.bfReserved1 = 0;
            //fh.bfReserved2 = 0;
            //WriteFile(hFile, &fh, sizeof(BITMAPFILEHEADER), &bytesWritten, NULL);

            //// 정보 헤더
            //BITMAPINFOHEADER ih;
            //ih.biSize = sizeof(BITMAPINFOHEADER);   // struct of BITMAPINFOHEADER
            //ih.biWidth = bw; // widht
            //ih.biHeight = bh; // height
            //ih.biPlanes = 1;
            //ih.biBitCount = 8;  // 8bit
            //ih.biCompression = BI_RGB;
            //ih.biSizeImage = 0;
            //ih.biXPelsPerMeter = 3780;  // pixels-per-meter
            //ih.biYPelsPerMeter = 3780;  // pixels-per-meter
            //ih.biClrUsed = 256;   // grayPalette count
            //ih.biClrImportant = 256;   // grayPalette count
            //WriteFile(hFile, &ih, sizeof(BITMAPINFOHEADER), &bytesWritten, NULL);

            //// RGB Palette
            //WriteFile(hFile, &grayPalette, sizeof(grayPalette), &bytesWritten, NULL);

            //// bmp파일은 파일 저장시 라인당 4byte padding을 한다.
            //// bw가 4로 나눠 떨어지지 않을경우 padding처리 해야 함
            //int fstep = (bw + 3) / 4 * 4;
            //int paddingSize = fstep - bw;
            //BYTE paddingBuf[] = {0,0,0,0};

            //// bmp파일은 위아래가 뒤집혀 있으므로 버퍼 아래라인 부터 읽어서 파일에 쓴다
            //for (int y = bh - 1; y >= 0; y--) {
            //    WriteFile(hFile, buf + y * bw, bw, &bytesWritten, NULL);
            //    if (paddingSize > 0)
            //        WriteFile(hFile, paddingBuf, paddingSize, &bytesWritten, NULL);
            //}

            //CloseHandle(hFile);
            return true;
        }

        // 이미지 파일 로드
        public unsafe static void LoadImageFile(string fileName, ref IntPtr imgBuf, ref int bw, ref int bh, ref int bytepp) {
            string ext = Path.GetExtension(fileName).ToLower();
            if (ext == ".bmp" || ext == ".jpg" || ext == ".png") {
                LoadBitmapFile(fileName, ref imgBuf, ref bw, ref bh, ref bytepp);
            } else if (ext == ".hra") {
                LoadHraFile(fileName, ref imgBuf, ref bw, ref bh, ref bytepp);
            }
        }

        // bmp, jpg, png
        public unsafe static void LoadBitmapFile(string fileName, ref IntPtr imgBuf, ref int bw, ref int bh, ref int bytepp) {
            using(var bmp = new Bitmap(fileName)) {
                bw = bmp.Width;
                bh = bmp.Height;
                if (bmp.PixelFormat == PixelFormat.Format8bppIndexed)
                    bytepp = 1;
                else if (bmp.PixelFormat == PixelFormat.Format16bppGrayScale)
                    bytepp = 2;
                else if (bmp.PixelFormat == PixelFormat.Format24bppRgb)
                    bytepp = 3;
                else if (bmp.PixelFormat == PixelFormat.Format32bppRgb || bmp.PixelFormat == PixelFormat.Format32bppArgb || bmp.PixelFormat == PixelFormat.Format32bppPArgb)
                    bytepp = 4;
                long bufSize = (long)bw * bh * bytepp;
                imgBuf = Marshal.AllocHGlobal(new IntPtr(bufSize));
                
                BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bw, bh), ImageLockMode.ReadOnly, bmp.PixelFormat);
                int copySize = bw * bytepp;
                for (int y = 0; y < bh; y++) {
                    IntPtr dstPtr = imgBuf + bw * y * bytepp;
                    IntPtr srcPtr = bmpData.Scan0 + bmpData.Stride * y;
                    Util.memcpy(dstPtr, srcPtr, copySize);
                }
                
                bmp.UnlockBits(bmpData);
            }
        }
        
        // hra
        public unsafe static void LoadHraFile(string fileName, ref IntPtr imgBuf, ref int bw, ref int bh, ref int bytepp) {
            FileStream sr;
            try {
                sr = File.OpenRead(fileName);
            } catch {
                return;
            }

            BinaryReader br = new BinaryReader(sr);
            sr.Position = 252;
            bytepp = br.ReadInt32();
            bw = br.ReadInt32();
            bh = br.ReadInt32();

            int bufSize = bw * bh * bytepp;
            imgBuf = Marshal.AllocHGlobal(bufSize);

            byte[] fbuf = br.ReadBytes(bufSize);
            for (int y = 0; y < bh; y++) {
                byte *dp = (byte*)imgBuf.ToPointer() + bw * bytepp * y;
                int idx = bw * bytepp * y;
                for (int x = 0; x < bw; x++, dp += bytepp, idx += bytepp) {
                    if (bytepp == 1)
                        dp[0] = fbuf[idx];
                    else if (bytepp == 2) {
                        dp[0] = fbuf[idx + 1];
                        dp[1] = fbuf[idx];
                    }
                }
            }

            br.Dispose();
            sr.Dispose();
        }

        // 이미지 버퍼를 디스플레이 버퍼에 복사
        public unsafe static void CopyImageBufferZoom(IntPtr sbuf, int sbw, int sbh, IntPtr dbuf, int dbw, int dbh, int panx, int pany, double zoom, int bytepp, int bgColor) {
            // 인덱스 버퍼 생성
            int[] siys = new int[dbh];
            int[] sixs = new int[dbw];
            for (int y = 0; y < dbh; y++) {
                int siy = (int)Math.Floor((y - pany) / zoom);
                siys[y] = (sbuf == IntPtr.Zero || siy < 0 || siy >= sbh) ? -1 : siy;
            }
            for (int x = 0; x < dbw; x++) {
                int six = (int)Math.Floor((x - panx) / zoom);
                sixs[x] = (sbuf == IntPtr.Zero || six < 0 || six >= sbw) ? -1 : six;
            }

            // dst 범위만큼 루프를 돌면서 해당 픽셀값 쓰기
            for (int y = 0; y < dbh; y++) {
                int siy = siys[y];
                byte* sp = (byte*)sbuf.ToPointer() + (Int64)sbw * siy * bytepp;
                int* dp = (int*)dbuf.ToPointer() + (Int64)dbw * y;
                for (int x = 0; x < dbw; x++, dp++) {
                    int six = sixs[x];
                    if (siy == -1 || six == -1) {
                        *dp = bgColor;
                    } else {
                        if (bytepp == 1) {
                            int gray8 = sp[six];
                            *dp = gray8 | gray8 << 8 | gray8 << 16 | 0xff << 24;
                        } else if (bytepp == 2) {
                            int gray8 = ((ushort*)sp)[six] >> 8;
                            *dp = gray8 | gray8 << 8 | gray8 << 16 | 0xff << 24;
                        } else if (bytepp == 3) {
                            byte* sptr = sp + six * 3;
                            byte* dptr = (byte*)dp;
                            dptr[2] = sptr[2];
                            dptr[1] = sptr[1];
                            dptr[0] = sptr[0];
                        } else {  // byte == 4
                            *dp = ((int*)sp)[six];
                        }
                    }
                }
            }
        }
    }
}
