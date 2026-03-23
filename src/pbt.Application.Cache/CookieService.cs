using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.Application.Cache
{
    public class CookieService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CookieService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // Add or update a cookie
        public void SetCookie(string key, string value, int? expireTimeMinutes = null)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return;

            var options = new CookieOptions
            {
                Expires = expireTimeMinutes.HasValue
                    ? DateTimeOffset.Now.AddMinutes(expireTimeMinutes.Value)
                    : DateTimeOffset.Now.AddDays(1)
            };

            // Ghi đè cookie, tự động gia hạn nếu đã tồn tại
            context.Response.Cookies.Append(key, value, options);
        }

        // Remove a cookie
        public void RemoveCookie(string key)
        {
            _httpContextAccessor.HttpContext?.Response.Cookies.Delete(key);
        }

        // Get a cookie value
        public string? GetCookie(string key)
        {
            return _httpContextAccessor.HttpContext?.Request.Cookies[key];
        }
    }
}
