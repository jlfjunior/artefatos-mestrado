using System.Net.Sockets;
using Xunit;

namespace Consolidado.IntegrationTests;

/// <summary>
/// [DockerDisponivelFact] roda o teste só quando há um Docker daemon acessível.
/// Sem Docker (ex.: build de CI sem Docker-in-Docker), o teste é marcado como
/// skip em vez de falhar. O código do teste continua compilando normalmente.
/// </summary>
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
        // Heurística simples: tenta o named pipe (Windows) / socket (Linux) do
        // Docker. Em vez de depender de toolchain, fazemos um probe rápido.
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
