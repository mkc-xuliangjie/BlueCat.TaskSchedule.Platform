using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace BlueCat.Core
{
    /// <summary>
    /// <see cref="HttpContext"/> 全局对象
    /// </summary>
    public static class HttpContextGlobal
    {
        static IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// 配置 HttpContext 访问者对象
        /// </summary>
        /// <param name="accessor"></param>
        public static void Configure(IHttpContextAccessor accessor)
        {
            _httpContextAccessor = accessor;
        }

        /// <summary>
        /// 获取当前的 <see cref="HttpContext"/> 对象
        /// </summary>
        public static HttpContext Current => _httpContextAccessor?.HttpContext;

        /// <summary>
        /// 获取当前请求的 TraceID 标识
        /// </summary>
        public static string CurrentTraceId
        {
            get
            {
                if (Current != null)
                {
                    if (Current.Request.Headers.TryGetValue(Constant.RESTfulTraceId, out Microsoft.Extensions.Primitives.StringValues value))
                    {
                        return value.ToString();
                    }
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// 请求URL
        /// </summary>
        public static string HostName
        {
            get
            {
                if (Current != null)
                {
                    return $"{Ip}:{Current.Request.Host.Port}{Current.Request.Path}";
                }

                return Ip;
            }
        }

        /// <summary>
        /// Ip地址
        /// </summary>
        private static string _ip;

        /// <summary>
        /// 设置Ip地址
        /// </summary>
        /// <param name="ip">Ip地址</param>
        public static void SetIp(string ip)
        {
            _ip = ip;
        }


        public static string Ip
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_ip) == false)
                    return _ip;
                var list = new[] { "127.0.0.1", "::1" };
                var result = Current?.Connection?.RemoteIpAddress?.ToString().Trim() ?? string.Empty; ;
                if (string.IsNullOrWhiteSpace(result) || list.Contains(result))
                    result = GetLanIp();
                return result;
            }
        }

        /// <summary>
        /// 获取局域网IP
        /// </summary>
        private static string GetLanIp()
        {
            try
            {
                foreach (var hostAddress in Dns.GetHostAddresses(Dns.GetHostName()))
                {
                    if (hostAddress.AddressFamily == AddressFamily.InterNetwork)
                        return hostAddress.ToString();
                }
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
