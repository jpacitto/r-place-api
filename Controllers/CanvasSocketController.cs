using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System.Net.NetworkInformation;

namespace r_place_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CanvasSocketController : ControllerBase
    {
        private readonly ILogger<CanvasSocketController> _logger;

        public CanvasSocketController(ILogger<CanvasSocketController> logger)
        {
            _logger = logger;
        }

        [HttpGet("memes")]
        public String Get()
        {
            var rng = new Random();
            Console.WriteLine("memes2");
            return "memes2";
        }

        [HttpGet("ws")]
        public async Task WebSocket()
        {
            GetTcpData();
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                _logger.Log(LogLevel.Information, "WebSocket connection established");
                await Echo(webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
            }
        }

        private async Task Echo(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            _logger.Log(LogLevel.Information, "Message received from Client");

            while (!result.CloseStatus.HasValue)
            {
                Console.WriteLine($"Message: {Encoding.UTF8.GetString(buffer)}");
                var serverMsg = Encoding.UTF8.GetBytes($"Server: Hello. You said: {Encoding.UTF8.GetString(buffer)}");
                await webSocket.SendAsync(new ArraySegment<byte>(serverMsg, 0, serverMsg.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);
                _logger.Log(LogLevel.Information, "Message sent to Client");

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                _logger.Log(LogLevel.Information, "Message received from Client");
            }

            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            _logger.Log(LogLevel.Information, "WebSocket connection closed");
        }

        private void GetTcpData()
        {
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            TcpStatistics tcpstat = properties.GetTcpIPv4Statistics();

            Console.WriteLine("  Connection Data:");
            Console.WriteLine("      Current  ............................ : {0}",
                tcpstat.CurrentConnections);
            Console.WriteLine("      Cumulative .......................... : {0}",
                tcpstat.CumulativeConnections);
            Console.WriteLine("      Initiated ........................... : {0}",
                tcpstat.ConnectionsInitiated);
            Console.WriteLine("      Accepted ............................ : {0}",
                tcpstat.ConnectionsAccepted);
            Console.WriteLine("      Failed Attempts ..................... : {0}",
                tcpstat.FailedConnectionAttempts);
            Console.WriteLine("      Reset ............................... : {0}",
                tcpstat.ResetConnections);
            Console.WriteLine("      Errors .............................. : {0}",
                tcpstat.ErrorsReceived);
            Console.WriteLine();
        }
    }
}
