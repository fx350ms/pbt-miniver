
namespace pbt.Packages;

using pbt.ApplicationUtils;
using System;
using System.Linq;
using System.Text;

public static class ShortProductCode
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    //public static string GenerateSafeAoId(int totalLength = 10)
    //{
    //    int randomPartLength = totalLength - PrefixConst.FakeOrderCode.Length;

    //    // Kết hợp ticks và random → tăng entropy
    //    long ticks = DateTime.UtcNow.Ticks;
    //    long randomPart = Random.Shared.NextInt64();
    //    long combined = (ticks << 16) ^ (randomPart & 0xFFFF_FFFF);

    //    // Encode sang base36 (gồm a-z + 0-9)
    //    string base36 = Base36Encode(combined);

    //    // Đảm bảo độ dài = 13 ký tự
    //    base36 = base36.PadLeft(randomPartLength, '0');
    //    if (base36.Length > randomPartLength)
    //        base36 = base36.Substring(0, randomPartLength);

    //    return PrefixConst.FakeOrderCode + base36;
    //}

    //static string Base36Encode(long value)
    //{
    //    const string chars = "0123456789";
    //    var length = chars.Length;
    //    StringBuilder result = new StringBuilder();
    //    value = Math.Abs(value);

    //    do
    //    {
    //        result.Insert(0, chars[(int)(value % length)]);
    //        value /= length;
    //    } while (value > 0);

    //    return result.ToString();
    //}

    public static string GenerateSafeAoId()
    {
        long ticksNow = DateTime.UtcNow.Ticks;

        long ticksBase = new DateTime(2025, 1, 1).Ticks;
        long diffMs = (ticksNow - ticksBase) / TimeSpan.TicksPerMillisecond;

        string code = $"{PrefixConst.FakeOrderCode}{diffMs:D9}{new Random().Next(10, 99)}";  
        return code;
    }
}