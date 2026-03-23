using MassTransit;
using Microsoft.Extensions.Configuration;
using pbt.EntityAuditLogs.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.EntityAuditLogs
{
    public interface IEntityAuditLogPublisher
    {
        Task PublishAsync(EntityAuditLogDto dto);
    }

    public interface IEntityAuditLogService
    {
        Task Publish(EntityAuditLogDto dto);
    }

   

    public class EntityAuditLogRabbitPublisher : IEntityAuditLogPublisher
    {
        private readonly IBus _bus;
        private readonly IConfiguration _config;

        public EntityAuditLogRabbitPublisher(IBus bus, IConfiguration config)
        {
            _bus = bus;
            _config = config;
        }

        public async Task PublishAsync(EntityAuditLogDto dto)
        {
            var queueName = _config["EntityAuditLog:QueueName"];

            var endpoint = await _bus.GetSendEndpoint(
                new Uri($"queue:{queueName}")
            );

            await endpoint.Send(dto);
            await _bus.Publish(dto);
        }
    }

    public class EntityAuditLogService : IEntityAuditLogService
    {
        private readonly IEntityAuditLogPublisher _publisher;

        public EntityAuditLogService(IEntityAuditLogPublisher publisher)
        {
            _publisher = publisher;
        }

        public async Task Publish(EntityAuditLogDto dto)
        {
            await _publisher.PublishAsync(dto);
        }
    }
}
