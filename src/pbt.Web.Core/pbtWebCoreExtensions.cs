using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace pbt
{
    public static class pbtWebCoreExtensions
    {
        public static string Get(this IEnumerable<Claim> claims, string claimType)
        {
            // Tìm kiếm Claim có ClaimType tương ứng và trả về giá trị của nó nếu tồn tại
            var claim = claims.FirstOrDefault(c => c.Type == claimType);

            // Nếu tìm thấy claim, trả về giá trị của nó. Nếu không, trả về null hoặc một giá trị mặc định.
            return claim?.Value ?? string.Empty; // hoặc có thể trả về null nếu không tìm thấy.
        }

    }
}
