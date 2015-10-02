using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.Xna.Framework;

namespace TurboPort
{

	/// <summary>
	/// Summary description for GravitiForceLevel.
	/// </summary>
	public class GravitiForceLevel
	{
        public static Hashtable types = new Hashtable();
        private static string levelPath;
        private static string typePath;
        private static string bitmapPath;

        public struct TypeOfLevel
        {
            public string Bitmap;
            public string Name;
            public string Title;
        }
        public string Name;
        public string FileName;
        public TypeOfLevel   type;
        public byte[] LevelData;
        public byte[] BitmapData;

        public Point[] playerBase;

        static GravitiForceLevel()
        {
            var prefix = typeof(GravitiForceLevel).Assembly.GetName().Name + ".Gf2Level.";
            levelPath = prefix + "Levels";
            typePath = prefix + "Types";
            bitmapPath = prefix + "Gfx";
        }

        protected GravitiForceLevel()
		{
		}

        public static TypeOfLevel GetTypeOfLevel(string typeLetters)
        {
            typeLetters = typeLetters.ToUpper();
            if(types.Contains(typeLetters))
            {
                return (TypeOfLevel)types[typeLetters];
            }
            byte[] fileData;
            Assembly assembly = typeof(GravitiForceLevel).Assembly;
            
            using(Stream stream = assembly.GetManifestResourceStream(typePath + "." + typeLetters))
            {
                fileData = new byte[stream.Length];
                stream.Read(fileData, 0, fileData.Length);
            }

            TypeOfLevel result = new TypeOfLevel();
            result.Bitmap = Encoding.ASCII.GetString(fileData, 0, Array.IndexOf(fileData, (byte)0, 0, 43));
            result.Name   = Encoding.ASCII.GetString(fileData, 43, Array.IndexOf(fileData, (byte)0, 43, 47) - 43);
            result.Title  = Encoding.ASCII.GetString(fileData, 90, Array.IndexOf(fileData, (byte)0, 90) - 90);

            types[typeLetters] = result;

            return result;
        }


        private byte[] Decrypt(byte[] inBuffer)
        {
            if(Encoding.ASCII.GetString(inBuffer, 0, 4) != "BPCK") // != compressed
            {
                return inBuffer;
            }
            byte[] outBuffer = new byte[(((inBuffer[8] * 256) + inBuffer[9]) * 256 + inBuffer[10]) * 256 + inBuffer[11]];
            byte commpressionMark1 = inBuffer[4];
            byte commpressionMark2 = inBuffer[5];
            int inOffset = 12;
            int outOffset = 0;

            while(inOffset < inBuffer.Length)
            {
                byte inByte = inBuffer[inOffset++];
                if(inByte == commpressionMark1)
                {
                    int length = inBuffer[inOffset++];
                    for(int i = 0; i < length; i++)
                    {
                        outBuffer[outOffset++] = inBuffer[inOffset];
                    }
                    inOffset++;
                }
                else if(inByte == commpressionMark2)
                {
                    int length = inBuffer[inOffset++] * 256 + inBuffer[inOffset++];
                    for(int i = 0; i < length; i++)
                    {
                        outBuffer[outOffset++] = inBuffer[inOffset];
                    }
                    inOffset++;
                }
                else
                {
                    outBuffer[outOffset++] = inByte;
                }
            }
            Trace.Assert(outOffset == outBuffer.Length, "Error decompressing ");
            return outBuffer;
        }

        public static GravitiForceLevel ReadGravitiForceLevelFile(string fileName)
        {
            GravitiForceLevel gfl = new GravitiForceLevel();


            gfl.FileName = fileName;
            gfl.Name = gfl.FileName.Substring(2, gfl.FileName.LastIndexOf('.') - 2).Replace("~3f", "?");
            gfl.type = GetTypeOfLevel(gfl.FileName.Substring(0, 2));

            byte[] inBuffer;
            Assembly assembly = typeof(GravitiForceLevel).Assembly;
            using(Stream stream = assembly.GetManifestResourceStream(levelPath + "." + fileName))
            {
                inBuffer = new byte[stream.Length];
                stream.Read(inBuffer, 0, inBuffer.Length);
            }
            gfl.LevelData = gfl.Decrypt(inBuffer);

            gfl.playerBase = new Point[2];
            gfl.playerBase[0] = new Point(gfl.LevelData[94]  * 256 + gfl.LevelData[95],  gfl.LevelData[98]  * 256 + gfl.LevelData[99]);
            gfl.playerBase[1] = new Point(gfl.LevelData[102] * 256 + gfl.LevelData[103], gfl.LevelData[106] * 256 + gfl.LevelData[107]);


            return gfl;
        }

        /*
        public Bitmap GetBitmap()
        {
            byte[] inBuffer;
            Assembly assembly = typeof(GravitiForceLevel).Assembly;
            using(Stream stream = assembly.GetManifestResourceStream(bitmapPath + "." + type.Bitmap))
            {
                inBuffer = new byte[stream.Length];
                stream.Read(inBuffer, 0, inBuffer.Length);
            }
            byte[] bitmapData = Decrypt(inBuffer);

            int blocks = (bitmapData.Length - 54 - 32) / 2 / 4 / 16;

            byte[] blockData = new byte[blocks * 16 * 16];

            int offsetSrc = 54;
            int offsetDst = 0;
            int planeOffset = (inBuffer == bitmapData) ? 2 : blocks * 2 * 16;
            for(int block = 0; block < blocks; block++)
            {
                for(int y = 0; y < 16; y++)
                {
                    int val0 = bitmapData[offsetSrc] * 256 + bitmapData[offsetSrc + 1];
                    offsetSrc += planeOffset;
                    int val1 = bitmapData[offsetSrc] * 256 + bitmapData[offsetSrc + 1];
                    offsetSrc += planeOffset;
                    int val2 = bitmapData[offsetSrc] * 256 + bitmapData[offsetSrc + 1];
                    offsetSrc += planeOffset;
                    int val3 = bitmapData[offsetSrc] * 256 + bitmapData[offsetSrc + 1];
                    offsetSrc -= planeOffset * 3;
                    offsetSrc += (inBuffer == bitmapData) ? 8 : 2;

                    for(int x = 0; x < 16; x++)
                    {
                        blockData[offsetDst++] = (byte)
                            (((val0 & 0x8000) >> 15) +
                            ((val1 & 0x8000) >> 14) +
                            ((val2 & 0x8000) >> 13) +
                            ((val3 & 0x8000) >> 12));
                        val0 += val0;
                        val1 += val1;
                        val2 += val2;
                        val3 += val3;
                    }
                }
            }
            byte[] colorMap = new byte[256 * 4];
            offsetSrc = bitmapData.Length - 16 * 2;
            offsetDst = 0;
            for(int i = 0; i < 16; i++)
            {
                colorMap[offsetDst++] = (byte)((bitmapData[++offsetSrc] & 0xf) * 0x11);
                colorMap[offsetDst++] = (byte)((bitmapData[offsetSrc--] & 0xf0) * 0x11 / 16);
                colorMap[offsetDst++] = (byte)((bitmapData[offsetSrc++] & 0x0f) * 0x11);
                colorMap[offsetDst++] = 0;
                offsetSrc++;
            }

            byte[] levelBitmap = new byte[21*63*16*16];
            for(int x = 0; x < 21; x++)
            {
                for(int y = 0; y < 63; y++)
                {
                    int block = LevelData[0x82 + x + (y * 21)];
                    if(block > 0)
                    {
                        offsetSrc = (block - 1) * 16 * 16;
                        offsetDst = levelBitmap.Length - (y * 21 * 16 * 16) - ((21 - x) * 16);
                        for(int i = 0; i < 16; i++)
                        {
                            for(int j = 0; j < 16; j++)
                            {
                                levelBitmap[offsetDst++] = blockData[offsetSrc++];
                            }
                            offsetDst -= 21 * 16 + 16;
                        }
                    }
                }
            }

            this.BitmapData = levelBitmap;

            byte[] bitmapHeader = new byte[54];
            bitmapHeader[0] = (byte)'B';
            bitmapHeader[1] = (byte)'M';
            int tmp = bitmapHeader.Length + colorMap.Length + levelBitmap.Length;
            bitmapHeader[2] = (byte)(tmp % 256);
            bitmapHeader[3] = (byte)((tmp / 256) % 256);
            bitmapHeader[4] = (byte)((tmp / 256 / 256) % 256);
            bitmapHeader[5] = (byte)((tmp / 256 / 256 / 256));
            bitmapHeader[6] = 0;
            bitmapHeader[7] = 0;
            bitmapHeader[8] = 0;
            bitmapHeader[9] = 0;
            tmp = bitmapHeader.Length + colorMap.Length;
            bitmapHeader[10] = (byte)(tmp % 256);
            bitmapHeader[11] = (byte)((tmp / 256) % 256);
            bitmapHeader[12] = (byte)((tmp / 256 / 256) % 256);
            bitmapHeader[13] = (byte)((tmp / 256 / 256 / 256));
            bitmapHeader[14] = 40;
            bitmapHeader[15] = 0;
            bitmapHeader[16] = 0;
            bitmapHeader[17] = 0;
            tmp = 21 * 16; // width
            bitmapHeader[18] = (byte)(tmp % 256);
            bitmapHeader[19] = (byte)((tmp / 256) % 256);
            bitmapHeader[20] = (byte)((tmp / 256 / 256) % 256);
            bitmapHeader[21] = (byte)((tmp / 256 / 256 / 256));
            tmp = 63 * 16; // height
            bitmapHeader[22] = (byte)(tmp % 256);
            bitmapHeader[23] = (byte)((tmp / 256) % 256);
            bitmapHeader[24] = (byte)((tmp / 256 / 256) % 256);
            bitmapHeader[25] = (byte)((tmp / 256 / 256 / 256));
            bitmapHeader[26] = 1; // planes 
            bitmapHeader[27] = 0; // planes 
            bitmapHeader[28] = 8; // bits per pixel
            bitmapHeader[29] = 0; // bits per pixel
            bitmapHeader[30] = 0; // compression
            bitmapHeader[31] = 0; // compression
            bitmapHeader[32] = 0; // compression
            bitmapHeader[33] = 0; // compression
            bitmapHeader[34] = 0;
            bitmapHeader[35] = 0;
            bitmapHeader[36] = 0;
            bitmapHeader[37] = 0;
            bitmapHeader[38] = 0;
            bitmapHeader[39] = 0;
            bitmapHeader[40] = 0;
            bitmapHeader[41] = 0;
            bitmapHeader[42] = 0;
            bitmapHeader[43] = 0;
            bitmapHeader[44] = 0;
            bitmapHeader[45] = 0;
            bitmapHeader[46] = 0;
            bitmapHeader[47] = 0;
            bitmapHeader[48] = 0;
            bitmapHeader[49] = 0;
            bitmapHeader[50] = 0;
            bitmapHeader[51] = 0;
            bitmapHeader[52] = 0;
            bitmapHeader[53] = 0;


            MemoryStream ms = new MemoryStream(bitmapHeader.Length + colorMap.Length + levelBitmap.Length);
            ms.Write(bitmapHeader, 0, bitmapHeader.Length);
            ms.Write(colorMap, 0, colorMap.Length);
            ms.Write(levelBitmap, 0, levelBitmap.Length);
            ms.Seek(0, SeekOrigin.Begin);
            System.Drawing.Bitmap bm = new System.Drawing.Bitmap(ms);
            //bm.Save(fullPath + ".png", System.Drawing.Imaging.ImageFormat.Png);
            ms.Close();

            return bm;
        }
        */
    }
}
