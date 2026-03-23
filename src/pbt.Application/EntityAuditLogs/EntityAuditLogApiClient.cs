using Castle.Core.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using pbt.ApplicationUtils;
using pbt.EntityAuditLogs.Dto;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace pbt.EntityAuditLogs
{
    public class EntityAuditLogApiClient : IEntityAuditLogApiClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        public EntityAuditLogApiClient(IHttpClientFactory httpClientFactory,
            ILogger logger,
            IConfiguration config
            )
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _config = config;
        }

        private async Task SendInternalAsync(EntityAuditLogDto dto)
        {
            var connectType = Convert.ToInt32(_config["EntityAuditLog:ConnectType"]);
            if (connectType != (int)EntityAuditLogConnectType.HttpApi)
            {
                _logger.Warn("EntityAuditLog is not using HttpApi connection type. Skip sending API.");
                return;
            }

            if (_httpClientFactory == null)
            {
                _logger.Error("HttpClientFactory is NULL. Cannot send EntityAuditLog.");
                return;
            }

            var stopWatchEnable = Convert.ToBoolean(_config["EntityAuditLog:StopWatchEnable"]);
            Stopwatch sw = new Stopwatch();
            if (stopWatchEnable) sw.Start();

            var client = _httpClientFactory.CreateClient("EntityAuditLogHttpClient");

            if (client.BaseAddress == null)
            {
                _logger.Error("HttpClient BaseAddress is NULL. Check EntityAuditLog:ApiUrl config.");
                return;
            }

            var tenantId = Convert.ToInt32(_config["EntityAuditLog:TenantId"]);
            var serviceName = Convert.ToString(_config["EntityAuditLog:ServiceName"]);

            dto.TenantId = tenantId;
            dto.ServiceName = serviceName;
            dto.CreatationTime = DateTime.Now;

            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(
                "/api/services/app/EntityAuditLog/Create",
                content
            );

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(); // ✅ FIX
                _logger.Error($"Call EntityAuditLog API failed: {response.StatusCode} - {error}");
            }

            if (stopWatchEnable)
            {
                sw.Stop();
                _logger.Info($"EntityAuditLogApiClient SendAsync took {sw.ElapsedMilliseconds} ms");
            }
        }
        public async Task SendAsync(EntityAuditLogDto dto)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    SendInternalAsync(dto);
                }
                catch (Exception ex)
                {
                    _logger.Error("Error when calling EntityAuditLog API (fire-and-forget): " + ex.Message, ex);
                }
            });

            
        }

    }
}
