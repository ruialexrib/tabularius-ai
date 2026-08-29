using Microsoft.AspNetCore.Identity;
using TabulariusAI.Web.Data.Identity;

namespace TabulariusAI.Web.Middleware;

/// <summary>Prevents an authenticated bootstrap administrator from accessing the application until the default password is replaced.</summary>
public sealed class MandatoryPasswordChangeMiddleware(RequestDelegate next)
{
    private const string BootstrapUserName = "admin";
    private const string BootstrapPassword = "LetMeIn";

    /// <summary>Processes the current request and redirects the bootstrap administrator to the mandatory password change page when required.</summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="userManager">The Identity user manager used to inspect the authenticated account.</param>
    /// <returns>A task representing the asynchronous middleware operation.</returns>
    public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager)
    {
        if (context.User.Identity?.IsAuthenticated == true && !IsAllowedPath(context.Request.Path))
        {
            var user = await userManager.GetUserAsync(context.User);
            if (user is not null &&
                string.Equals(user.UserName, BootstrapUserName, StringComparison.OrdinalIgnoreCase) &&
                await userManager.CheckPasswordAsync(user, BootstrapPassword))
            {
                context.Response.Redirect("/Account/ChangePassword");
                return;
            }
        }

        await next(context);
    }

    /// <summary>Determines whether a request must remain accessible while the bootstrap password replacement is pending.</summary>
    /// <param name="path">The requested application path.</param>
    /// <returns>True for password replacement and sign-out endpoints; otherwise false.</returns>
    private static bool IsAllowedPath(PathString path) =>
        path.StartsWithSegments("/Account/ChangePassword", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWithSegments("/Account/Logout", StringComparison.OrdinalIgnoreCase);
}
