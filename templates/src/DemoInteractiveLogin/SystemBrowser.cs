﻿using IdentityModel.OidcClient.Browser;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DemoInteractiveLogin
{
    public class SystemBrowser : IBrowser
    {
        public readonly int _port;
        private readonly string? _path;

        public SystemBrowser(int port, string? path = null)
        {
            _port = port;
            _path = path;
        }

        public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken)
        {
            using var listener = new LoopbackHttpListener(_port, _path);
            OpenBrowser(options.StartUrl);

            try
            {
                var result = await listener.WaitForCallbackAsync();
                if (string.IsNullOrWhiteSpace(result))
                {
                    return new BrowserResult { ResultType = BrowserResultType.UnknownError, Error = "Empty response." };
                }

                return new BrowserResult { Response = result, ResultType = BrowserResultType.Success };
            }
            catch (TaskCanceledException ex)
            {
                return new BrowserResult { ResultType = BrowserResultType.Timeout, Error = ex.Message };
            }
            catch (Exception ex)
            {
                return new BrowserResult { ResultType = BrowserResultType.UnknownError, Error = ex.Message };
            }
        }

        public static void OpenBrowser(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
