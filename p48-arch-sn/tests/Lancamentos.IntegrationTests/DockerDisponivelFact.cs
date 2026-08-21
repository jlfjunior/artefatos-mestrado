using System.Net.Sockets;
using Xunit;

namespace Lancamentos.IntegrationTests;

public sealed class DockerDisponivelFact : FactAttribute
{
    public DockerDisponivelFact()
    {
        if (!DockerCheck.Disponivel())
            Skip = "Docker não está disponível neste ambiente.";
    }
}

internal static class DockerCheck
{
    private static readonly Lazy<bool> _disponivel = new(Detectar);

    public static bool Disponivel() => _disponivel.Value;

    private static bool Detectar()
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                return File.Exists(@"\\.\pipe\docker_engine")
                    || Directory.GetFiles(@"\\.\pipe\").Any(p => p.Contains("docker", StringComparison.OrdinalIgnoreCase));
            }

            const string unixSocket = "/var/run/docker.sock";
            if (File.Exists(unixSocket))
            {
                using var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
                socket.Connect(new UnixDomainSocketEndPoint(unixSocket));
                return socket.Connected;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }
}
