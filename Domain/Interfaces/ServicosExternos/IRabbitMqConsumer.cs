using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces.ServicosExternos
{
    public interface IRabbitMqConsumer
    {
        void Consume(string queue, Func<string, Task> handleMessage);
    }
}
