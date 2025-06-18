using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Channels;
using Booking.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Booking.Infrastructure.SSE;

public sealed class SSEPublisher : ISSEPublisher
{
    private record SseClient(Channel<string> Channel, CancellationTokenSource Cts);

    private readonly ConcurrentDictionary<string, SseClient> _clients = new();

    public async Task RegisterClientAsync(string sessionId, HttpContext context)
    {
        var response = context.Response;

        // Thiết lập header; dùng indexer để không ném ngoại lệ nếu header đã tồn tại
        response.Headers["Content-Type"] = "text/event-stream; charset=utf-8";
        response.Headers["Cache-Control"] = "no-cache";
        response.Headers["Connection"] = "keep-alive";

        // Tạo channel & CTS gắn với RequestAborted
        var channel = Channel.CreateUnbounded<string>();
        var cts = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted);

        // Lưu client
        _clients[sessionId] = new SseClient(channel, cts);

        // Gửi ping mỗi 10 s để giữ kết nối
        _ = Task.Run(async () =>
        {
            try
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(10), cts.Token);
                    await channel.Writer.WriteAsync(": ping\n\n", cts.Token);
                }
            }
            catch (OperationCanceledException) { /* bỏ qua */ }
        });

        // Đọc tất cả thông điệp và ghi tuần tự xuống Response
        try
        {
            await foreach (var msg in channel.Reader.ReadAllAsync(cts.Token))
            {
                await response.WriteAsync(msg, cts.Token);
                await response.Body.FlushAsync(cts.Token);
            }
        }
        catch (OperationCanceledException) { /* client đóng */ }
        catch (IOException) { /* client đóng đột ngột */ }
        finally
        {
            // Dọn dẹp
            _clients.TryRemove(sessionId, out _);
            channel.Writer.TryComplete();
            cts.Dispose();
        }
    }

    public async Task SendAsync(string sessionId, string eventName, object data)
    {
        if (_clients.TryGetValue(sessionId, out var client))
        {
            var payload =
                $"event: {eventName}\n" +
                $"data: {JsonSerializer.Serialize(data)}\n\n";

            // Viết vào channel để đảm bảo thứ tự và tránh ghi song song
            try
            {
                await client.Channel.Writer.WriteAsync(payload, client.Cts.Token);
            }
            catch (OperationCanceledException) { /* client đã rời */ }
        }
    }
}
