using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.Commons.Dto
{
    public class ApiResonseDto<T>
    {
        public int Code { get; set; } // Mã kết quả xử lý. 1: Thành công, 0: Thất bại
        public string Message { get; set; } // Thông điệp
        public T Data { get; set; }

    }

    public class ApiResonseDto
    {
        public int Code { get; set; } // Mã kết quả xử lý. 1: Thành công, 0: Thất bại
        public string Message { get; set; } // Thông điệp
    }
}
