using Finbuckle.MultiTenant;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Finbuckle.ObjectOptions
{
    public class CustomConfigurationStore : IMultiTenantStore
    {
        internal const string defaultSectionName = "Finbuckle:MultiTenant:Stores:ConfigurationStore";
        private readonly IConfigurationSection section;
        private ConcurrentDictionary<string, TenantInfo> tenantMap;

        public CustomConfigurationStore(IConfiguration configuration) : this(configuration, defaultSectionName)
        {
        }

        public CustomConfigurationStore(IConfiguration configuration, string sectionName)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (string.IsNullOrEmpty(sectionName))
            {
                throw new ArgumentException("Section name provided to the Configuration Store is null or empty.", nameof(sectionName));
            }

            section = configuration.GetSection(sectionName);
            if (!section.Exists())
            {
                throw new MultiTenantException("Section name provided to the Configuration Store is invalid.");
            }

            UpdateTenantMap();
            ChangeToken.OnChange(() => section.GetReloadToken(), UpdateTenantMap);
        }

        private void UpdateTenantMap()
        {
            var newMap = new ConcurrentDictionary<string, TenantInfo>(StringComparer.OrdinalIgnoreCase);
            var tenants = section.GetSection("Tenants").GetChildren();

            //foreach (var tenantSection in tenants)
            //{
            //    var newTenant = section.GetSection("Defaults").Get<TenantInfo>(options => options.BindNonPublicProperties = true);
            //    tenantSection.Bind(newTenant, options => options.BindNonPublicProperties = true);
            //    newMap.TryAdd(newTenant.Identifier, newTenant);
            //}

            foreach (var tenantSection in tenants)
            {
                var defaultsSection = section.GetSection("Defaults");

                var newTenant = defaultsSection.Get<DerivedTenantInfo>(options =>
                {
                    options.BindNonPublicProperties = true;
                });
                tenantSection.Bind(newTenant, options => options.BindNonPublicProperties = true);
                newMap.TryAdd(newTenant.Identifier, newTenant);
            }

            var oldMap = tenantMap;
            tenantMap = newMap;
        }

        public Task<bool> TryAddAsync(TenantInfo tenantInfo)
        {
            throw new NotImplementedException();
        }

        public async Task<TenantInfo> TryGetAsync(string id)
        {
            if (id is null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            return await Task.FromResult(tenantMap.Where(kv => kv.Value.Id == id).SingleOrDefault().Value);
        }

        public async Task<TenantInfo> TryGetByIdentifierAsync(string identifier)
        {
            if (identifier is null)
            {
                throw new ArgumentNullException(nameof(identifier));
            }

            return await Task.FromResult(tenantMap.TryGetValue(identifier, out var result) ? result : null);
        }

        public Task<bool> TryRemoveAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> TryUpdateAsync(TenantInfo tenantInfo)
        {
            throw new NotImplementedException();
        }

        public class ConfigurationStoreOptions
        {
            TenantInfo Defaults { get; set; }
            public List<TenantInfo> Tenants { get; set; }
        }
    }
}
