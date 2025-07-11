using Domain.Interfaces.ServicosExternos;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.ServicosExternos
{
    public class EmailNotificationServiceFake : INotificationService
    {
        private readonly IConfiguration _config;

        public EmailNotificationServiceFake(IConfiguration config)
        {
            _config = config;
        }

        public async Task NotifyUserAsync(string email, string subject, string message)
        {
            return;
        }
    }
}
