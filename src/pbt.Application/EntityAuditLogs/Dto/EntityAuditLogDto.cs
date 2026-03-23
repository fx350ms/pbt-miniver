using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.EntityAuditLogs.Dto
{
    public class EntityAuditLogDto
    {
        /// <summary>
        /// Id của thực thể bị ảnh hưởng
        /// </summary>
        public string EntityId { get; set; }

        /// <summary>
        /// Tenant Id của thực thể bị ảnh hưởng
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// Dịch vụ
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// Hàm thực thi: Tạo mới, Cập nhật, Xóa, ...
        /// </summary>
        public string MethodName { get; set; }

        /// <summary>
        /// Tên thực thể bị ảnh hưởng: Kiện, bao, đơn hàng,....
        /// </summary>
        public string EntityType { get; set; }

        /// <summary>
        /// Tiêu đề: Kiện hàng #123456 được tạo mới
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// Ngày tạo
        /// </summary>
        public DateTime CreatationTime { get; set; }

        public long CreatationTimeInt { get; set; }

        /// <summary>
        /// Id người dùng thực hiện
        /// </summary>
        public long? UserId { get; set; }

        /// <summary>
        /// Tên người dùng thực hiện
        /// </summary>
        public string UserName { get; set; }
    }
}
