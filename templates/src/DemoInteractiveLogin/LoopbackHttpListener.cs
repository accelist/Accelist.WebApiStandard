using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace DemoInteractiveLogin
{
    public class LoopbackHttpListener : IDisposable
    {
        const int DefaultTimeout = 60 * 5; // 5 mins (in seconds)

        private readonly IWebHost _host;
        private readonly TaskCompletionSource<string> _source = new();
        private readonly string? _path;
        private readonly string _url;

        public string Url => _url;

        public LoopbackHttpListener(int port, string? path = null)
        {
            _path = path; 

            _url = $"http://127.0.0.1:{port}";

            _host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls(_url)
                .Configure(Configure)
                .Build();
            _host.Start();
        }

        public void Dispose()
        {
            Task.Run(async () =>
            {
                await Task.Delay(500);
                _host.Dispose();
            });
            GC.SuppressFinalize(this);
        }

        void Configure(IApplicationBuilder app)
        {
            if (string.IsNullOrEmpty(_path) == false)
            {
                app.UsePathBase(_path);
            }

            app.Run(async ctx =>
            {
                if (ctx.Request.Method == "GET")
                {
                    await SetResultAsync(ctx.Request.QueryString.Value ?? "", ctx);
                }
                else
                {
                    ctx.Response.StatusCode = 405;
                }
            });
        }

        private async Task SetResultAsync(string value, HttpContext ctx)
        {
            _source.TrySetResult(value);

            try
            {
                ctx.Response.StatusCode = 200;
                ctx.Response.ContentType = "text/html";
                await ctx.Response.WriteAsync("<h1>You can now return to the application.</h1>");
                await ctx.Response.Body.FlushAsync();
            }
            catch
            {
                ctx.Response.StatusCode = 400;
                ctx.Response.ContentType = "text/html";
                await ctx.Response.WriteAsync("<h1>Invalid request.</h1>");
                await ctx.Response.Body.FlushAsync();
            }
        }

        public Task<string> WaitForCallbackAsync(int timeoutInSeconds = DefaultTimeout)
        {
            Task.Run(async () =>
            {
                await Task.Delay(timeoutInSeconds * 1000);
                _source.TrySetCanceled();
            });

            return _source.Task;
        }
    }
}
