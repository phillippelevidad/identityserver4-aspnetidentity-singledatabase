using Identity.Api.Models;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Extensions;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.EntityFramework.Options;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Identity.Api.Data
{
    public class ApplicationDbContext :
        IdentityDbContext<ApplicationUser>,
        IPersistedGrantDbContext,
        IConfigurationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        #region IConfigurationDbContext

        public DbSet<Client> Clients { get; set; }
        public DbSet<ClientCorsOrigin> ClientCorsOrigins { get; set; }
        public DbSet<IdentityResource> IdentityResources { get; set; }
        public DbSet<ApiResource> ApiResources { get; set; }
        public DbSet<ApiScope> ApiScopes { get; set; }

        #endregion

        #region IPersistedGrantDbContext

        public DbSet<PersistedGrant> PersistedGrants { get; set; }
        public DbSet<DeviceFlowCodes> DeviceFlowCodes { get; set; }
        public Task<int> SaveChangesAsync() => base.SaveChangesAsync();

        #endregion

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            ConfigurePersistentGrantDbContext(builder);
            ConfigureConfigurationDbContext(builder);
        }

        private void ConfigurePersistentGrantDbContext(ModelBuilder builder)
        {
            var options = new OperationalStoreOptions();
            SetSchemaForAllTables(options, "isgrants");

            builder.ConfigurePersistedGrantContext(options);
        }

        private void ConfigureConfigurationDbContext(ModelBuilder builder)
        {
            var options = new ConfigurationStoreOptions();
            SetSchemaForAllTables(options, "isconfig");

            builder.ConfigureClientContext(options);
            builder.ConfigureResourcesContext(options);
        }

        private void SetSchemaForAllTables<T>(T options, string schema)
        {
            var tableConfigurationType = typeof(TableConfiguration);
            var schemaProperty = tableConfigurationType.GetProperty(nameof(TableConfiguration.Schema));

            var tableConfigurations = options.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => tableConfigurationType.IsAssignableFrom(property.PropertyType))
                .Select(property => property.GetValue(options, null));

            foreach (var table in tableConfigurations)
                schemaProperty.SetValue(table, schema, null);
        }
    }
}
