using EasyNetQ;
using System;
using System.Threading.Tasks;

namespace Core.Bus
{
    public interface IMessageBus : IDisposable
    {
        bool IsConnected { get; }
        IAdvancedBus AdvancedBus { get; }

        Task PublishAsync<T>(T message) where T : Message;

        void SubscribeAsync<T>(string subscriptionId, Func<T, Task> onMessage) where T : class;

        Task<TResponse> RequestAsync<TRequest, TResponse>(TRequest request)
            where TRequest : Message
            where TResponse : ResponseMessage;

        IDisposable RespondAsync<TRequest, TResponse>(Func<TRequest, Task<TResponse>> responder)
            where TRequest : Message
            where TResponse : ResponseMessage;
    }
}
