using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Cube.Core.Pool;

namespace Cube.Core.Network;

public class TcpSendChannel : SendChannel<TcpSendContext>
{
    public TcpSendChannel(ILoggerFactory loggerFactory, PoolEvent poolEvent) : base(loggerFactory, poolEvent) { }

    protected override async Task OnProcessAsync(TcpSendContext ctx)
    {
        var saea = RentSocketAsyncEventArgs();
        saea.RemoteEndPoint = ctx.Socket.RemoteEndPoint;
        saea.SetBuffer(ctx.Data);
        saea.UserToken = ctx;

        try
        {
            if (!ctx.Socket.SendToAsync(saea))
            {
                OnSendCompletedAsync(null, saea);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[TcpSendChannel] Error sending packet");
        }
        finally
        {
            ctx.Return();
        }

        await Task.CompletedTask;
    }

    protected override void OnSendCompletedAsync(object? sender, SocketAsyncEventArgs e)
    {
        if (e.UserToken is TcpSendContext ctx)
        {
            ctx.OnSendCompleted?.Invoke(ctx);
        }

        ReturnSocketAsyncEventArgs(e);
    }
}
