using System;
using System.Globalization;

namespace pbt.ApplicationUtils
{
    public static class Extensions
    {
        /// <summary>
        /// Chuyển một giá trị số sang chuỗi định dạng tiền tệ theo culture và ký hiệu tiền tệ.
        /// Ví dụ:
        /// decimal x = 1234567.89M;
        /// x.ToCurrencyFormat(); // với mặc định "vi-VN" và "₫" => "1.234.567,89 ₫"
        /// int y = 5000;
        /// y.ToCurrencyFormat("en-US", "$"); // => "$5,000.00"
        /// </summary>
        /// <typeparam name="T">Kiểu giá trị (số)</typeparam>
        /// <param name="value">Giá trị cần định dạng</param>
        /// <param name="culture">Chuỗi culture (ví dụ "vi-VN", "en-US")</param>
        /// <param name="currencySymbol">Ký hiệu tiền tệ (ví dụ "₫", "$")</param>
        /// <returns>Chuỗi đã được định dạng theo tiền tệ</returns>
        public static string ToCurrencyFormat<T>(this T value, string culture = "vi-VN", string currencySymbol = "₫")
        {   
            var cultureInfo = new CultureInfo(culture)
            {
                NumberFormat = { CurrencySymbol = currencySymbol }
            };
            // Nếu T không phải là kiểu nguyên thủy và giá trị không phải decimal thì trả về "0" theo định dạng tiền tệ
            if (!typeof(T).IsPrimitive && !(value is decimal))
            {
                return string.Format(cultureInfo, "{0:C}", 0);
            }
            return string.Format(cultureInfo, "{0:C}", value);
        }

        /// <summary>
        /// Định dạng số theo phần nghìn, mẫu: "#,##0".
        /// Ví dụ:
        /// int a = 1234567;
        /// a.ToThousandFormat(); // "1,234,567"
        /// </summary>
        /// <typeparam name="T">Kiểu struct và IConvertible (số)</typeparam>
        /// <param name="value">Giá trị cần định dạng</param>
        /// <returns>Chuỗi định dạng với phân cách phần nghìn</returns>
        /// <exception cref="ArgumentException">Nếu kiểu không phải là kiểu số hỗ trợ</exception>
        public static string ToThousandFormat<T>(this T value) where T : struct, IConvertible
        {
            if (!typeof(T).IsPrimitive && !(value is decimal))
            {
                throw new ArgumentException("Only numeric types are supported.");
            }
            return string.Format("{0:#,##0}", value);
        }

        /// <summary>
        /// Chuẩn hóa số để xuất Excel:
        /// - Nếu là số nguyên => không in phần thập phân.
        /// - Nếu có phần thập phân => in 1 chữ số thập phân.
        /// Ví dụ:
        /// object v1 = 100.0m; => "100"
        /// object v2 = 12.34m; => "12.3"
        /// </summary>
        /// <typeparam name="T">Kiểu bất kỳ (nhưng thường là numeric)</typeparam>
        /// <param name="value">Giá trị cần chuẩn hóa</param>
        /// <returns>Chuỗi đã chuẩn hóa phù hợp cho Excel</returns>
        public static string ToExcelNumberFormat<T>(this T value)
        {
            if (value == null) return string.Empty;

            if (decimal.TryParse(value.ToString(), out decimal number))
            {
                // Kiểm tra nếu số là số nguyên
                if (number % 1 == 0)
                {
                    return ((decimal)number).ToString("0");
                }
                else
                {
                    return number.ToString("0.0"); // Hiển thị 1 chữ số thập phân
                }
            }

            return value.ToString();
        }

        /// <summary>
        /// Định dạng số theo chuẩn tùy chỉnh:
        /// - Sử dụng dấu phân tách phần nghìn là ',' và phân tách thập phân là '.'
        /// - Mẫu: "#,##0.##"
        /// Ví dụ:
        /// decimal z = 1234567.89M;
        /// z.ToCustomNumberFormat(); // "1,234,567.89"
        /// </summary>
        /// <typeparam name="T">Kiểu giá trị (số)</typeparam>
        /// <param name="value">Giá trị cần định dạng</param>
        /// <returns>Chuỗi định dạng theo quy tắc custom</returns>
        /// <exception cref="ArgumentException">Nếu kiểu không phải số</exception>
        public static string ToCustomNumberFormat<T>(this T value)
        {
            if (!typeof(T).IsPrimitive && !(value is decimal))
            {
                throw new ArgumentException("Only numeric types are supported.");
            }

            // Chuyển đổi sang decimal để xử lý phần thập phân
            decimal number = Convert.ToDecimal(value);

            // Định nghĩa culture tùy chỉnh: phần nghìn = "," và thập phân = "."
            var cultureInfo = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            cultureInfo.NumberFormat.NumberGroupSeparator = ",";
            cultureInfo.NumberFormat.NumberDecimalSeparator = ".";

            return number.ToString("#,##0.##", cultureInfo);
        }
    }
}
