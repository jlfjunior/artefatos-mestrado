using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flow.Application.MessageBrokers
{
    public interface IKafkaProducer
    {
        Task ProduceAsync<T>(T message);
    }
}
