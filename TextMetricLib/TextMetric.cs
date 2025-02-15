using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextMetricLib
{
    public class TextMetric
    {
        // not used
        public uint Symbol
        {
            get; set;
        }
        public string Text
        {
            get; set;
        }
        public double FontSize
        {
            get; set;
        }
        public double MarginLeft
        {
            get; set;
        }
        public double MarginTop
        {
            get; set;
        }
        // 1 Grid.Column = 1 scale_degree
        // Grid.Column = scale_degree + ScaleOffset
        public int Row
        {
            get; set;
        }
        public int Column
        {
            get; set;
        }
        public int RowSpan
        {
            get; set;
        }
        // Grid.ColumnSpan = ScaleSpan
        public int ColumnSpan
        {
            get; set;
        }
        public TextMetric(uint symbol, string text, double fontSize, double marginLeft, double marginTop, int row, int column, int rowSpan, int columnSpan)
        {
            Symbol = symbol;
            Text = text;
            FontSize = fontSize;
            MarginLeft = marginLeft;
            MarginTop = marginTop;
            Row = row;
            Column = column;
            RowSpan = rowSpan;
            ColumnSpan = columnSpan;
        }
        public static List<TextMetric> Data = new List<TextMetric>
        {
            //new TextMetric(0x1D100, "1D10", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄀", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄁", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄂", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄃", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄄", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄅", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄆", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄇", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄈", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄉", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄊", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄋", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄌", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄍", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄎", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄏", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "1D11", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄐", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄑", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄒", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄓", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄔", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄕", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄖", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄗", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄘", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄙", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄚", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄛", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄜", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄝", 10, 0, 0, 0, 0, 0, 0),
            new TextMetric(0x1D100, "𝄞", 512, -21, -124/*-456*/, -3, -2, 11, 5),
            //new TextMetric(0x1D100, "𝄟", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "1D12", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄠", 10, 0, 0, 0, 0, 0, 0),
            new TextMetric(0x1D100, "𝄡", 520, -53, -161/*-462*/, -4, -2, 8, 5),
            new TextMetric(0x1D100, "𝄢", 512/*463*/, -21, -230/*-461*/, -3, -2, 7, 5),
            //new TextMetric(0x1D100, "𝄣", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄤", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄥", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄦", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄩", 10, 0, 0, 0, 0, 0, 0),
            new TextMetric(0x1D100, "𝄪", 394, -15, -148, -1, 0, 2, 2),
            new TextMetric(0x266D, "♭", 283, -25, -107, -3, -1, 6, 2),
            new TextMetric(0x266E, "♮", 283, -25, -57, -3, -1, 6, 2),
            new TextMetric(0x266F, "♯", 283, -5, -57, -3, -1, 6, 2),
            new TextMetric(0x1D100, "𝄫", 283, -25, -107, -3, -1, 6, 3),
            //new TextMetric(0x1D100, "𝄬", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄭", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄮", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄯", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "1D13", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄰", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄱", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄲", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄳", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄴", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄵", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄶", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄷", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄸", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄹", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄺", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄻", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄼", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄽", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄾", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝄿", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "1D14", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝅀", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝅁", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝅂", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝅃", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝅄", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝅅", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝅆", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝅇", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝅈", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝅉", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝅊", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝅋", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝅌", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝅍", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝅎", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝅏", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "1D15", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝅐", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝅑", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝅒", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝅓", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝅔", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝅕", 450, 0, -390, 0, 0),
            //new TextMetric(0x1D100, "𝅖", 450, 0, -390, 0, 0),
            new TextMetric(0x1D100, "𝅗", 449, -40, -390, -1, -1, 2, 2),
            new TextMetric(0x1D100, "𝅘", 449, -40, -390, -1, -1, 2, 2),
            //new TextMetric(0x1D100, "𝅙", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝅚", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝅛", 10, 0, 0, 0, 0, 0, 0),
            new TextMetric(0x1D100, "𝅜", 432, -14, -369, -1, -2, 2, 5),
            new TextMetric(0x1D100, "𝅝", 432, -18, -369, -1, -1, 2, 3),
            //new TextMetric(0x1D100, "𝅗𝅥", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝅘𝅥", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "1D16", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝅘𝅥𝅮", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝅘𝅥𝅯", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝅘𝅥𝅰", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝅘𝅥𝅱", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝅘𝅥𝅲", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝅥", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝅦", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝅧", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝅨", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝅩", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝅪", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝅫", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝅬", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝅭", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝅮", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝅯", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "1D17", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝅰", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝅱", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝅲", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝅻", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝅼", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝅽", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝅾", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝅿", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "1D18", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆀", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆁", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆂", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆃", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆄", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆅", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆆", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆇", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆈", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆉", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆊", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆋", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆌", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆍", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆎", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆏", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "1D19", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆐", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆑", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆒", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆓", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆔", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆕", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆖", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆗", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆘", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆙", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆚", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆛", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆜", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆝", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆞", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆟", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "1D1A", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆠", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆡", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆢", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆣", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆤", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆥", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆦", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆧", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆨", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆩", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆪", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆫", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆬", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆭", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆮", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆯", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "1D1B", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆰", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆱", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆲", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆳", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆴", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆵", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆶", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆷", 10, 0, 0, 0, 0, 0, 0),
            new TextMetric(0x1D100, "𝆸", 282, -12, -205, -1, -1, 2, 3),
            new TextMetric(0x1D100, "𝆹", 336, -13, -269, -1, -1, 2, 2),
            new TextMetric(0x1D100, "𝆺", 336, -13, -269, -1, -1, 2, 2),
            //new TextMetric(0x1D100, "𝆹𝅥", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆺𝅥", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆹𝅥𝅮", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆺𝅥𝅮", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆹𝅥𝅯", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "1D1C", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝆺𝅥𝅯", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝇁", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝇂", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝇃", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝇄", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝇅", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝇆", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝇇", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝇈", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝇉", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝇊", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝇋", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝇌", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝇍", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝇎", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝇏", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "1D1D", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝇐", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝇑", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝇒", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝇓", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝇔", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝇕", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝇖", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝇗", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝇘", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝇙", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝇚", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝇛", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝇜", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝇝", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝇞", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝇟", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "1D1E", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝇠", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝇡", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝇢", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝇣", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝇤", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝇥", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝇦", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝇧", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric(0x1D100, "𝇨", 10, 0, 0, 0, 0, 0, 0),
            new TextMetric(0x1D16D,"𝅭", 100, 51, -23,-1, -1, 3, 2),
            new TextMetric(0x1D16D,"-", 100, 12.5 * .5, 0, 0, -1, 1, 4),
            new TextMetric(0x1D16D,"_-", 100, 12.5 * .5, 0, 0, -3, 1, 6),
            new TextMetric(0x1D16D,"-_", 100, 0, 0,-1, -1, 2, 6),
            new TextMetric(0x1D16D,"0", 138, /*44*/-5, -51,-1, -1, 2, 6),
            new TextMetric(0x1D16D,"1", 69, /*44*/-5, -5,-1, -1, 2, 2),

            //new TextMetric('\u0000', "𝄞", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric('\u0000', "𝄡", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric('\u0000', "𝄢", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric('\u0000', "𝄪", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric('\u0000', "𝄫", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric('\u0000', "𝅗", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric('\u0000', "𝅘", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric('\u0000', "𝅜", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric('\u0000', "𝅝", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric('\u0000', "𝆸", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric('\u0000', "𝆹", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric('\u0000', "𝆺", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric('\u266D', "♭", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric('\u266E', "♮", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric('\u266F', "♯", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric('\u0000', "♩", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric('\u0000', "♪", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric('\u0000', "⬛", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric('\u0000', "⬜", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric('\u0000', "⬥", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric('\u0000', "⬦", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric('\u0000', "⬬", 10, 0, 0, 0, 0, 0, 0),
            //new TextMetric('\u0000', "⬭", 10, 0, 0, 0, 0, 0, 0),
        };
    }
}
