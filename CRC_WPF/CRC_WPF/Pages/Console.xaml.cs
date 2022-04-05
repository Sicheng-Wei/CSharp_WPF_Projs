using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CRC_WPF
{
    /// <summary>
    /// Console.xaml 的交互逻辑
    /// </summary>
    public partial class Console : Page
    {
        // 信息比特
        private static byte[]? MessageByte;
        private static byte[]? CheckByte;
        private static byte[]? MessageRecv;
        private static byte[]? CheckRecv;

        // 零宽占位字符
        private static readonly byte[] ascii_null = { 0x00 };
        private static readonly byte[] zero_width = { 0x0B, 0x20 };

        // 位翻转辅助
        private static readonly byte[] bit_flipper = { 0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01 };

        // 输出速度调节
        private static readonly int correct_spd = 20;
        private static readonly int error_spd = 200;
        private static readonly int fatal_spd = 3000;

        public Console()
        {
            InitializeComponent();
        }

        // 清空
        private void Clear_AllClick(object sender, RoutedEventArgs e)
        {
            //Clear_All();
        }
        private void Clear_F4Pressed(object sender, ExecutedRoutedEventArgs e)
        {
            Clear_All();
        }
        private void Clear_All()
        {
            SenderInformation.Clear();
            HexString.Text = "";
            EncodedHex.Text = "";
            HexReceive.Text = "";
            OriginHex.Text = "";
            DownText.Text = "";
            
            MessageByte = null;
            MessageRecv = null;
            CheckByte = null;
            CheckRecv = null;
        }

        // 上传字符串
        private void Input_UploadClick(object sender, RoutedEventArgs e)
        {
            //Now_Upload();
        }
        private void Upload_F5Pressed(object sender, ExecutedRoutedEventArgs e)
        {
            Now_Upload();
        }
        private void Now_Upload()
        {
            // 关闭按钮功能
            Silence();

            string StrInput = SenderInformation.Text;

            // Binary
            if (InputFormat.SelectedIndex == 0)
            {
                try
                {
                    StrInput = "1" + StrInput;
                    StrInput = StrInput.PadLeft((int)Math.Ceiling(StrInput.Length / 64.0) * 64, '0');
                    MessageByte = new byte[StrInput.Length / 8];
                    for (int i = 0; i < MessageByte.Length; i++)
                    {
                        MessageByte[i] = Convert.ToByte(StrInput.Substring(i * 8, 8), 2);
                    }
                    HexString.Text = BitConverter.ToString(MessageByte).Replace("-"," ");
                    Encode_1.ScrollToEnd();
                }
                catch (Exception)
                {
                    Color color = (Color) ColorConverter.ConvertFromString("#ce1126");
                    SenderInformation.Foreground = new SolidColorBrush(color);
                    Pause(fatal_spd);
                    color = (Color) ColorConverter.ConvertFromString("#000000");
                    SenderInformation.Foreground = new SolidColorBrush(color);
                }
            }

            // ASCII
            else if (InputFormat.SelectedIndex == 1)
            {
                bool isASCII = true;
                foreach (char ch in StrInput)
                {
                    if (ch > 255)
                    {
                        isASCII = false;
                        break;
                    }
                }
                if (isASCII)
                {
                    MessageByte = Encoding.ASCII.GetBytes(StrInput);
                    HexString.Text = "";
                    while (MessageByte.Length % 8 != 0)
                    {
                        MessageByte = MessageByte.Concat(ascii_null).Where(a => a >= 0).ToArray();
                    }
                    // 8 byte 标准输出
                    for (int i = 0; i < MessageByte.Length; i += 8)
                    {
                        HexString.Text += BitConverter.ToString(MessageByte[i..(i + 8)]).Replace("-", " ");
                        HexString.Text += "\n";
                        Encode_1.ScrollToEnd();
                        Pause(correct_spd);
                    }

                }
                else
                {
                    Color color = (Color) ColorConverter.ConvertFromString("#ce1126");
                    SenderInformation.Foreground = new SolidColorBrush(color);
                    Pause(fatal_spd);
                    color = (Color) ColorConverter.ConvertFromString("#000000");
                    SenderInformation.Foreground = new SolidColorBrush(color);
                }

            }

            // Unicode
            else
            {
                try
                {
                    MessageByte = Encoding.Unicode.GetBytes(StrInput);
                    HexString.Text = "";

                    // 8 byte 标准化
                    while (MessageByte.Length % 8 != 0)
                    {
                        MessageByte = MessageByte.Concat(zero_width).Where(a => (int)a >= 0).ToArray();
                    }

                    // 8 byte 标准输出
                    for(int i = 0; i < MessageByte.Length; i += 8)
                    {
                        HexString.Text += BitConverter.ToString(MessageByte[i..(i + 8)]).Replace("-", " ");
                        HexString.Text += '\n';
                        Encode_1.ScrollToEnd();
                        Pause(correct_spd);
                    }
                }
                catch (Exception)
                {
                    Color color = (Color)ColorConverter.ConvertFromString("#ce1126");
                    SenderInformation.Foreground = new SolidColorBrush(color);
                    Pause(fatal_spd);
                    color = (Color)ColorConverter.ConvertFromString("#000000");
                    SenderInformation.Foreground = new SolidColorBrush(color);
                }
            }

            // 关闭按钮功能
            Hustler();
        }

        // 编码 + 显示
        private void With_EncodeClick(object sender, RoutedEventArgs e)
        {
            // Now_Encode();
        }
        private void Encode_F6Pressed(object sender, ExecutedRoutedEventArgs e)
        {
            Now_Encode();
        }
        private void Now_Encode()
        {
            // 关闭按钮功能
            Silence();

            byte[] PacketCRC = { 0x00, 0x00 };
            EncodedHex.Text = "";
            CheckByte = new byte[0];
            if (MessageByte == null) return;
            switch (CRCType.SelectedIndex)
            {
                case 0:
                    for (int i = 0; i < MessageByte.Length; i += 8)
                    {
                        PacketCRC[1] = CRC_Encoder.crc4(MessageByte[i..(i + 8)]);
                        CheckByte = CheckByte.Concat(PacketCRC).Where(a => a >= 0).ToArray();
                        EncodedHex.Text += BitConverter.ToString(MessageByte[i..(i + 8)]).Replace("-", " ") + "\t";
                        EncodedHex.Text += BitConverter.ToString(PacketCRC).Replace("-", " ");
                        EncodedHex.Text += '\n';
                        Encode_2.ScrollToEnd();
                        Pause(correct_spd);
                    }
                    break;
                case 1:
                    for (int i = 0; i < MessageByte.Length; i += 8)
                    {
                        PacketCRC[1] = CRC_Encoder.crc5(MessageByte[i..(i + 8)]);
                        CheckByte = CheckByte.Concat(PacketCRC).Where(a => a >= 0).ToArray();
                        EncodedHex.Text += BitConverter.ToString(MessageByte[i..(i + 8)]).Replace("-", " ") + "\t";
                        EncodedHex.Text += BitConverter.ToString(PacketCRC).Replace("-", " ");
                        EncodedHex.Text += '\n';
                        Encode_2.ScrollToEnd();
                        Pause(correct_spd);
                    }
                    break;
                case 2:
                    for (int i = 0; i < MessageByte.Length; i += 8)
                    {
                        PacketCRC = BitConverter.GetBytes(CRC_Encoder.crc16(MessageByte[i..(i + 8)]));
                        CheckByte = CheckByte.Concat(PacketCRC).Where(a => a >= 0).ToArray();
                        EncodedHex.Text += BitConverter.ToString(MessageByte[i..(i + 8)]).Replace("-", " ") + "\t";
                        EncodedHex.Text += BitConverter.ToString(PacketCRC).Replace("-", " ");
                        EncodedHex.Text += '\n';
                        Encode_2.ScrollToEnd();
                        Pause(correct_spd);
                    }
                    break;
                default:
                    return;
            }

            // 重新使能按钮
            Hustler();
        }

        // 传输控制台 + 验证码
        private void Loss_SliderDrag(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            int value = (int) this.LossSlider.Value;
            this.LossOne.Text = (value % 10).ToString();
            this.LossTen.Text = (value / 10).ToString();
        }
        private void With_TransmitClick(object sender, RoutedEventArgs e)
        {
            // Transmit_LossPass();
            // Check_CRC();
        }
        private void Transmit_F7Pressed(object sender, ExecutedRoutedEventArgs e)
        {
            Transmit_LossPass();
        }
        private void Transmit_LossPass()
        {
            // 关闭按钮功能
            Silence();

            int LossThresh = (int)LossSlider.Value;
            HexReceive.Text = "";
            OriginHex.Text = "";
            DownText.Text = "";

            if (MessageByte == null || CheckByte == null) return;
            // 收到数据
            MessageRecv = new byte[MessageByte.Length];
            CheckRecv = new byte[CheckByte.Length];

            // 生成新种子
            Random rand = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);

            for (int i = 0; i < MessageRecv.Length; i += 8)
            {
                // 数据字节的随机错误
                for (int j = 0; j < 8; j++)
                {
                    MessageRecv[i + j] = MessageByte[i + j];
                    if (rand.Next(1000) < LossThresh)
                        MessageRecv[i + j] = Bit_Flip(MessageRecv[i + j], j);
                }

                // CRC字节的随机错误
                for (int j = 0; j < 2; j++)
                {
                    CheckRecv[i / 4 + j] = CheckByte[i / 4 + j];
                    if (rand.Next(1000) < LossThresh)
                        CheckRecv[i / 4 + j] = Bit_Flip(CheckRecv[i / 4 + j], j);
                }

                bool isValid = Check_CRC(MessageRecv[i..(i + 8)], CheckRecv[(i / 4)..(i / 4 + 2)]);
                if (isValid)
                {
                    HexReceive.Text += BitConverter.ToString(MessageRecv[i..(i + 8)]).Replace("-", " ") + "\t";
                    HexReceive.Text += BitConverter.ToString(CheckRecv[(i / 4)..(i / 4 + 2)]).Replace("-", " ") + "\n";
                    OriginHex.Text += BitConverter.ToString(MessageRecv[i..(i + 8)]).Replace("-", " ") + "\n";
                    Retriv_Message(MessageRecv[i..(i + 8)], i == 0 && InputFormat.SelectedIndex == 0);
                    Decode_1.ScrollToEnd();
                    Decode_2.ScrollToEnd();
                    Pause(correct_spd);
                }
                else
                {
                    HexReceive.Text += "Transmit Error: Need Resending!\n";
                    Pause(error_spd);
                    i -= 8;
                }
            }

            // 重新使能
            Hustler();
        }
        private static byte Bit_Flip(byte data, int index)
        {
            byte temp = data;
            bool flag = (byte)(temp << index) >= 0x80;
            if (flag)
            {
                return (byte) (data - bit_flipper[index]);
            }
            else
            {
                return (byte)(data + bit_flipper[index]);
            }
        }
        private bool Check_CRC(byte[] message, byte[] check)
        {
            byte[]? packet = Byte_Refine(message, check, CRCType.SelectedIndex);
            if (packet == null) return true;
            switch (CRCType.SelectedIndex)
            {
                case 0:
                    return CRC_Encoder.crc4(packet) == 0;
                case 1:
                    return CRC_Encoder.crc5(packet) == 0;
                case 2:
                    return CRC_Encoder.crc16(packet) == 0;
            }
            return true;
        }
        private static byte[]? Byte_Refine(byte[] message, byte[] check, int crc_type)
        {
            byte[] refine;
            switch (crc_type)
            {
                case 0: // crc4
                    refine = new byte[message.Length + check.Length - 1];
                    for (int i = 0; i < message.Length; i++)
                        refine[i] = message[i];
                    refine[refine.Length - 1] = check[1];
                    return refine;
                case 1: // crc5
                    refine = new byte[message.Length + check.Length - 1];
                    for (int i = 0; i < message.Length; i++)
                        refine[i] = message[i];
                    refine[refine.Length - 1] = check[1];
                    return refine;
                case 2: // crc16
                    refine = new byte[message.Length + check.Length];
                    for (int i = 0; i < message.Length; i++)
                        refine[i] = message[i];
                    refine[refine.Length - 2] = check[0];
                    refine[refine.Length - 1] = check[1];
                    return refine;
            }
            return null;
        }
        private void Retriv_Message(byte[] packet, bool isBnFst)
        {
            switch (InputFormat.SelectedIndex)
            {
                case 0:
                    string pacstr = "";
                    for(int i = 0; i < packet.Length; i++)
                        pacstr += (Convert.ToString(packet[i], 2)).PadLeft(8, '0');
                    if (isBnFst)
                    {
                        int fst = pacstr.IndexOf('1');
                        DownText.Text += pacstr[(fst + 1)..pacstr.Length];
                        isBnFst = false;
                    }
                    else
                        DownText.Text += pacstr;
                    break;
                case 1:
                    DownText.Text += Encoding.ASCII.GetString(packet);
                    break;
                case 2:
                    DownText.Text += Encoding.Unicode.GetString(packet);
                    break;
            }
            Download.ScrollToEnd();

        }

        // 暂停函数
        private static void Pause(int milisec)
        {
            var t = DateTime.Now.AddMilliseconds(milisec);
            while (DateTime.Now < t)
            {
                DispatcherHelper.DoEvents();
            }
        }

        // 使能与禁能
        private void Silence()
        {
            Btn_Clear.IsEnabled = false;
            Btn_Encode.IsEnabled = false;
            Btn_Transmit.IsEnabled = false;
            Btn_Upload.IsEnabled = false;
        }
        private void Hustler()
        {
            Btn_Clear.IsEnabled = true;
            Btn_Encode.IsEnabled = true;
            Btn_Transmit.IsEnabled = true;
            Btn_Upload.IsEnabled = true;
        }
    }

    public static class DispatcherHelper
    {
        public static void DoEvents()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(ExitFrames), frame);
            try { Dispatcher.PushFrame(frame); }
            catch (InvalidOperationException) { }
        }
        private static object? ExitFrames(object frame)
        {
            ((DispatcherFrame)frame).Continue = false;
            return null;
        }
    }

}
