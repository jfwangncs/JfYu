using System.ComponentModel;
using System.Globalization;

namespace JfYu.UnitTests.Models
{
    public class AllTypeTestModel
    {
        public short Short { get; set; }
        public ushort Ushort { get; set; }
        public int Int { get; set; }
        public uint Uint { get; set; }
        public long Long { get; set; }
        public ulong Ulong { get; set; }
        public float Float { get; set; }
        public double Double { get; set; }
        public decimal Decimal { get; set; }
        public bool Bool { get; set; }
        public string String { get; set; } = null!;
        public DateTime DateTime { get; set; }
        public byte Byte { get; set; }
        public sbyte Sbyte { get; set; }
        public string? EmptyStr { get; set; }
        public string? NullStr { get; set; }

        [DisplayName("NullShort")]
        public short? NullShort { get; set; }

        public ushort? NullUshort { get; set; }
        public int? NullInt { get; set; }
        public uint? NullUint { get; set; }
        public long? NullLong { get; set; }
        public ulong? NullUlong { get; set; }
        public float? NullFloat { get; set; }
        public double? NullDouble { get; set; }
        public decimal? NullDecimal { get; set; }
        public bool? NullBool { get; set; }
        public string? NullString { get; set; }
        public DateTime? NullDateTime { get; set; }
        public byte? NullByte { get; set; }
        public sbyte? NullSbyte { get; set; }

        public static List<AllTypeTestModel> GenerateTestList()
        {
            return [ new AllTypeTestModel
                {
                    Short = short.MaxValue,
                    Ushort = ushort.MaxValue,
                    Int = int.MaxValue,
                    Uint = uint.MaxValue,
                    Long = long.MaxValue,
                    Ulong = ulong.MaxValue,
                    Float = float.MaxValue,
                    Double = double.MaxValue,
                    Decimal = decimal.MaxValue,
                    Bool = true,
                    String = "Hello, World!",
                    DateTime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),CultureInfo.InvariantCulture),
                    Byte = byte.MaxValue,
                    Sbyte = sbyte.MaxValue,
                    EmptyStr = "",
                    NullStr = null,

                    NullShort = short.MinValue,
                    NullUshort = ushort.MaxValue,
                    NullInt = int.MinValue,
                    NullUint = uint.MaxValue,
                    NullLong = long.MinValue,
                    NullUlong = ulong.MaxValue,
                    NullFloat = -float.MaxValue,
                    NullDouble = -double.MaxValue,
                    NullDecimal = decimal.MinValue,
                    NullBool = false,
                    NullString = null,
                    NullDateTime = DateTime.Parse(DateTime.MinValue.ToString("yyyy-MM-dd HH:mm:ss.fff"),CultureInfo.InvariantCulture),
                    NullByte = byte.MinValue,
                    NullSbyte = sbyte.MinValue
                },new AllTypeTestModel
                {
                    Short = 0,
                    Ushort = 0,
                    Int = 0,
                    Uint = 0,
                    Long = 0,
                    Ulong = 0,
                    Float = 0.0f,
                    Double = 0.0,
                    Decimal = 0m,
                    Bool = false,
                    String = "Another string",
                    DateTime = DateTime.Parse(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff"),CultureInfo.InvariantCulture),
                    Byte = 0,
                    Sbyte = 0,
                    EmptyStr = "",
                    NullStr = "Not null",

                    NullShort = null,
                    NullUshort = null,
                    NullInt = null,
                    NullUint = null,
                    NullLong = null,
                    NullUlong = null,
                    NullFloat = null,
                    NullDouble = null,
                    NullDecimal = null,
                    NullBool = null,
                    NullString = "Nullable string with value",
                    NullDateTime = null,
                    NullByte = null,
                    NullSbyte = null
                },new AllTypeTestModel
                {
                    Short = 12345,
                    Ushort = 12345,
                    Int = 123456789,
                    Uint = 123456789u,
                    Long = 1234567890123456789L,
                    Ulong = 1234567890123456789uL,
                    Float = 123.456f,
                    Double = 123.456,
                    Decimal = 123.456m,
                    Bool = true,
                    String = "Random string",
                    DateTime = DateTime.Parse(new DateTime(2023, 1, 1,0,0,0, DateTimeKind.Local).ToString("yyyy-MM-dd HH:mm:ss.fff"),CultureInfo.InvariantCulture),
                    Byte = 128,
                    Sbyte = -128,
                    EmptyStr = "Not empty",
                    NullStr = "Also not null",

                    NullShort = 12345,
                    NullUshort = 12345,
                    NullInt = 123456789,
                    NullUint = 123456789u,
                    NullLong = 1234567890123456789L,
                    NullUlong = 1234567890123456789uL,
                    NullFloat = 123.456f,
                    NullDouble = 123.456,
                    NullDecimal = 123.456m,
                    NullBool = true,
                    NullString = "Nullable string with another value",
                    NullDateTime = DateTime.Parse(new DateTime(2023, 1, 1,0,0,0, DateTimeKind.Local).ToString("yyyy-MM-dd HH:mm:ss.fff"),CultureInfo.InvariantCulture),
                    NullByte = 128,
                    NullSbyte = -128
                },new AllTypeTestModel
                {
                    String = "!@#$%^&*()_+{}:\"<>?[];',./~`",
                    EmptyStr = " ",
                    NullStr = "Special chars"
                },new AllTypeTestModel
                {
                    Short = short.MinValue + 1,
                    Ushort = 1,
                    Int = int.MinValue + 1,
                    Uint = 1,
                    Long = long.MinValue + 1,
                    Ulong = 1,
                    Float = float.Epsilon,
                    Double = double.Epsilon,
                    Decimal = decimal.MinusOne,
                    Bool = false,
                    String = "Very small numbers",
                    DateTime = DateTime.Parse(DateTime.MaxValue.ToString("yyyy-MM-dd HH:mm:ss.fff"), CultureInfo.InvariantCulture),
                    Byte = 1,
                    Sbyte = -1,
                    EmptyStr = " ",
                    NullStr = "Small numbers",

                    NullShort = 1,
                    NullUshort = 1,
                    NullInt = 1,
                    NullUint = 1,
                    NullLong = 1,
                    NullUlong = 1,
                    NullFloat = float.Epsilon,
                    NullDouble = double.Epsilon,
                    NullDecimal = decimal.MinusOne,
                    NullBool = false,
                    NullString = "Very small nullable numbers",
                    NullDateTime = DateTime.Parse(DateTime.MaxValue.ToString("yyyy-MM-dd HH:mm:ss.fff"), CultureInfo.InvariantCulture),
                    NullByte = 1,
                    NullSbyte = -1
                },new AllTypeTestModel
                {
                    NullShort = null,
                    NullUshort = null,
                    NullInt = null,
                    NullUint = null,
                    NullLong = null,
                    NullUlong = null,
                    NullFloat = null,
                    NullDouble = null,
                    NullDecimal = null,
                    NullBool = null,
                    NullString = null,
                    NullDateTime = null,
                    NullByte = null,
                    NullSbyte = null
                } ];
        }
    }
}