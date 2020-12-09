using System;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Risk.gRPC
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;
        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply {
                Message = "Hello " + request.Name
            });
        }

        public override Task<AddReply> Add(AddRequest request, ServerCallContext context)
        {
            return Task.FromResult(new AddReply {
                Answer = request.Num1 + request.Num2
            });
        }

        public override async Task StreamingFromServer(StreamingRequest request, IServerStreamWriter<StreamingReply> responseStream, ServerCallContext context)
        {
            var responseNumber = 1;
            var started = DateTime.Now;
            while (!context.CancellationToken.IsCancellationRequested)
            {
                await responseStream.WriteAsync(new StreamingReply {
                    CurrentTimestamp = DateTime.Now.ToString(),
                    Message = $"This is a streaming message # {responseNumber++}",
                    SecondsElapsed = (int)((DateTime.Now - started).TotalSeconds)
                });
                await Task.Delay(TimeSpan.FromSeconds(1), context.CancellationToken);
            }
        }

        public override async Task StreamBothWays(IAsyncStreamReader<StreamingRequest> requestStream, IServerStreamWriter<StreamingReply> responseStream, ServerCallContext context)
        {
            var readTask = Task.Run(async () =>
            {
                await foreach (var message in requestStream.ReadAllAsync())
                {
                    //process request
                    _logger.LogInformation($"got message {message.ClientName}/{message.FavoriteNumber}");
                }
            });

            while(!readTask.IsCompleted)
            {
                _logger.LogInformation("sending reply");
                await responseStream.WriteAsync(new StreamingReply() { CurrentTimestamp = DateTime.Now.ToString() });
                await Task.Delay(TimeSpan.FromSeconds(1), context.CancellationToken);
            }
        }
    }
}
