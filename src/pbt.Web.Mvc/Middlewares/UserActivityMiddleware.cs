using Abp.Runtime.Security;
using Microsoft.AspNetCore.Authentication;
using pbt.Authorization.Users;
using pbt.Identity;
namespace pbt.Web.Middlewares;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

public class UserActivityMiddleware : ITransientDependency
{
    private readonly RequestDelegate _next;
    private readonly SignInManager _signInManager;

    public UserActivityMiddleware(RequestDelegate next, SignInManager signInManager)
    {
        _next = next;
        _signInManager = signInManager;
    }

    public async Task Invoke(HttpContext context, UserManager<User> userManager)
    {
        if (context.User.Identity.IsAuthenticated)
        {
            var userId = context.User.Claims.FirstOrDefault(c => c.Type == AbpClaimTypes.UserId)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user != null && !user.IsActive)
                {
                    // 👉 SignOut để clear cookie
                    await _signInManager.SignOutAsync();
                    await context.SignOutAsync(IdentityConstants.ApplicationScheme);

                    // 👉 Redirect về trang login
                    context.Response.Redirect("/Account/Login");
                    return;
                }
            }
        }

        await _next(context);
    }
}