using System.Net;
using System.Net.Http;
using System.Net.Sockets;

namespace Dotnetable.Infrastructure.Social;

/// <summary>Social API calls leave through the worker on IPv4 and HTTP/1.1.</summary>
static class SocialHttp
{
    public static void ConfigureClient(HttpClient client)
    {
        client.DefaultRequestVersion = new Version(1, 1);
        client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
        client.DefaultRequestHeaders.ExpectContinue = false;
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Dotnetable");
    }

    public static SocketsHttpHandler CreateHandler() => new()
    {
        AutomaticDecompression = System.Net.DecompressionMethods.All,
        PooledConnectionLifetime = TimeSpan.FromMinutes(2),
        ConnectCallback = ConnectIpv4,
    };

    private static async ValueTask<Stream> ConnectIpv4(SocketsHttpConnectionContext context, CancellationToken cancellationToken)
    {
        var addresses = await Dns.GetHostAddressesAsync(context.DnsEndPoint.Host, cancellationToken);
        var ipv4 = addresses.Where(address => address.AddressFamily == AddressFamily.InterNetwork).ToArray();
        var chosen = ipv4.Length > 0 ? ipv4 : addresses;
        var socket = new Socket(chosen[0].AddressFamily, SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
        try
        {
            await socket.ConnectAsync(chosen, context.DnsEndPoint.Port, cancellationToken);
            return new NetworkStream(socket, ownsSocket: true);
        }
        catch
        {
            socket.Dispose();
            throw;
        }
    }
}
