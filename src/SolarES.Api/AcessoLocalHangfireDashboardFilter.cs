using Hangfire.Dashboard;

namespace SolarES.Api;

/// <summary>
/// Restringe o dashboard do Hangfire (/hangfire) a requisicoes da propria maquina.
/// O dashboard e navegado direto no browser (sem o Authorization: Bearer da API), e
/// construir uma ponte JWT-para-pagina-navegavel e um problema de auth maior do que
/// a T21 deveria resolver agora -- revisar quando houver deploy publico de verdade.
/// </summary>
public sealed class AcessoLocalHangfireDashboardFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var ip = context.GetHttpContext().Connection.RemoteIpAddress;
        return ip is not null && System.Net.IPAddress.IsLoopback(ip);
    }
}
