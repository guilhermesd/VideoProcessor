using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces.ServicosExternos
{
    public interface IRabbitMqProducer
    {
        void Publish(string queue, string message);
    }

}
