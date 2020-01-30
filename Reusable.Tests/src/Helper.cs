using System.Collections.Immutable;
using Microsoft.Extensions.Caching.Memory;
using Reusable.Translucent;
using Reusable.Translucent.Middleware;

namespace Reusable
{
    public abstract class TestHelper
    {
        public static readonly string ConnectionString = "Data Source=(local);Initial Catalog=TestDb;Integrated Security=SSPI;";

        public static readonly IResourceRepository Resources = new ResourceRepository<TestResourceSetup>(ImmutableServiceProvider.Empty.Add<IMemoryCache>(new MemoryCache(new MemoryCacheOptions())));

        private class TestResourceSetup
        {
            public void ConfigureResources(IResourceCollection resources)
            {
                resources.AddEmbeddedFile<TestHelper>(ControllerName.Empty, @"Reusable/res/Beaver");
                resources.AddEmbeddedFile<TestHelper>(ControllerName.Empty, @"Reusable/res/Translucent");
                resources.AddEmbeddedFile<TestHelper>(ControllerName.Empty, @"Reusable/res/Flexo");
                resources.AddEmbeddedFile<TestHelper>(ControllerName.Empty, @"Reusable/res/Utilities/JsonNet");
                resources.AddEmbeddedFile<TestHelper>(ControllerName.Empty, @"Reusable/sql");
                resources.AddAppConfig();
                resources.AddSqlServer(ControllerName.Empty, ConnectionString, sql =>
                {
                    sql.TableName = ("reusable", "TestConfig");
                    sql.ColumnMappings =
                        ImmutableDictionary<SqlServerColumn, SoftString>
                            .Empty
                            .Add(SqlServerColumn.Name, "_name")
                            .Add(SqlServerColumn.Value, "_value");
                    sql.Where =
                        ImmutableDictionary<string, object>
                            .Empty
                            .Add("_env", "test")
                            .Add("_ver", "1");
                    sql.Fallback =
                        ImmutableDictionary<string, object>
                            .Empty
                            .Add("_env", "else");
                });
            }

            public void ConfigurePipeline(IPipelineBuilder<ResourceContext> pipeline)
            {
                pipeline.UseMiddleware<CacheMiddleware>();
                pipeline.UseMiddleware<SettingValidationMiddleware>();
                pipeline.UseMiddleware<ResourceExistsValidationMiddleware>();
            }
        }
    }
}