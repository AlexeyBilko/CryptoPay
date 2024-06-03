using ServiceLayer.Service.Realization.IdentityServices;

namespace CryptoPaymentGateway.Middleware
{
    public class TokenVersionValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public TokenVersionValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                var userId = context.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    var userService = context.RequestServices.GetService(typeof(UserService)) as UserService;
                    var user = await userService.FindByIdAsync(userId);
                    var tokenVersionClaim = context.User.Claims.FirstOrDefault(c => c.Type == "TokenVersion")?.Value;
                    int tokenVersion = tokenVersionClaim != null ? int.Parse(tokenVersionClaim) : -1;

                    if (user != null && user.TokenVersion != tokenVersion)
                    {
                        // If the token version doesn't match, consider this token invalid
                        context.Response.StatusCode = 401; // Unauthorized
                        await context.Response.WriteAsync("Invalid token version");
                        return; // Stop further processing of this request
                    }
                }
            }

            // Continue processing if the token is valid
            await _next(context);
        }
    }
}
