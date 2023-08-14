using Microsoft.AspNetCore.Http;

namespace ThreeL.Shared.Application.Middlewares
{
    /// <summary>
    /// 静态文件目录访问中间件
    /// </summary>
    public class AuthorizeStaticFilesMiddleware
    {
        RequestDelegate _next;
        private readonly string _url;

        public AuthorizeStaticFilesMiddleware(RequestDelegate next, string folder)
        {
            _next = next;
            _url = folder;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments(_url))
            {
                if (context.User.Identity.IsAuthenticated)
                {
                    await _next(context);
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                }
            }
            else
            {
                await _next(context);
            }
        }
    }
}
