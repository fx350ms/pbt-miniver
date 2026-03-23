using System;
using System.ComponentModel;

namespace pbt.ApplicationUtils
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            // Lấy kiểu của enum
            var type = value.GetType();

            // Lấy tên của enum value
            var name = Enum.GetName(type, value);

            if (name != null)
            {
                // Lấy trường thông tin enum
                var field = type.GetField(name);

                if (field != null)
                {
                    // Lấy attribute DescriptionAttribute
                    var attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;

                    if (attr != null)
                    {
                        // Trả về nội dung description
                        return attr.Description;
                    }
                }
            }

            // Nếu không có Description, trả về tên của enum
            return name;
        }
    }
}
