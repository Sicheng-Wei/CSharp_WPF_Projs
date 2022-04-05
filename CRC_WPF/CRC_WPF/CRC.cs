using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRC_WPF
{
    public static class CRC_Encoder
    {
        public static byte crc4(byte[] data)
        {
            // 多项式：10011（省略最高位为0x03）
            return crc8_common(data, data.Length, 4, 0x03);
        }

        public static byte crc5(byte[] data)
        {
            // 多项式：110101（省略最高位为0x15）
            return crc8_common(data, data.Length, 3, 0x15);
        }

        public static ushort crc16(byte[] data)
        {
            // 多项式：0x18005（省略最高位为0x8005）
            return crc16_common(data, data.Length, 0, 0x8005);
        }


        private static byte crc8_common(byte[] data, int len, byte shift, byte poly)
        {
            byte crc = 0x00;
            poly = (byte)(poly << shift);
            byte data_byte;

            for (int j = 0; j < len; j++)
            {
                data_byte = reverse_8(data[j]);
                crc = (byte)(crc ^ data_byte);
                for (int i = 0; i < 8; i++)
                {
                    if ((crc & 0x80) != 0)
                    {
                        crc = (byte)((crc << 1) ^ poly);
                    }
                    else
                    {
                        crc = (byte)(crc << 1);
                    }
                }
            }
            crc = reverse_8(crc);
            return crc;
        }
        private static ushort crc16_common(byte[] data, int len, ushort shift, ushort poly)
        {
            ushort crc = 0x0000;
            poly = (ushort)(poly << shift);
            byte data_byte;

            for (int j = 0; j < len; j++)
            {
                data_byte = reverse_8(data[j]);
                crc = (ushort)(crc ^ (data_byte << 8));
                for (int i = 0; i < 8; i++)
                {
                    if ((crc & 0x8000) != 0)
                    {
                        crc = (ushort)((crc << 1) ^ poly);
                    }
                    else
                    {
                        crc = (ushort)(crc << 1);
                    }
                }
            }
            crc = reverse_16(crc);
            return crc;
        }

        private static byte reverse_8(byte data)
        {
            byte i;
            byte temp = 0;
            for (i = 0; i < 8; i++)
                temp |= (byte)(((data >> i) & 0x01) << (7 - i));
            return temp;
        }
        private static ushort reverse_16(ushort data)
        {
            ushort i;
            ushort temp = 0;
            for (i = 0; i < 16; i++)
                temp |= (ushort)(((data >> i) & 0x01) << (15 - i));
            return temp;
        }
    }
}

