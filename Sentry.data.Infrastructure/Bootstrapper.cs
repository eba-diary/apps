using Hangfire;
using Nest;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Mapping.ByCode;
using Polly.Registry;
using Sentry.data.Core;
using Sentry.data.Core.Entities.Schema.Elastic;
using Sentry.data.Core.Interfaces;
using Sentry.data.Core.Interfaces.SAIDRestClient;
using Sentry.data.Infrastructure.Mappings.Primary;
using Sentry.data.Infrastructure.PollyPolicies;
using Sentry.Messaging.Common;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Infrastructure
{
    /// <summary>
    /// This class is responsible for wiring up the Infrastructure to the application.
    /// </summary>
    public static class Bootstrapper
    {

        private static StructureMap.IContainer _container;
        private static ISessionFactory _defaultSessionFactory;

        /// <summary>
        /// <para>The Structure Map dependency resolver.  </para>
        /// <para>Each request should be done within a nested container 
        /// that you retrieve from <see cref="StructureMap.IContainer.GetNestedContainer"/>.
        /// For your web application, this is already done for you by the StructureMapMvcDependencyResolver.</para>
        /// </summary>
        public static StructureMap.IContainer Container
        {
            get
            {
                return _container;
            }
        }

        /// <summary>
        /// The session factory for the default datastore for this application.
        /// </summary>
        /// <remarks>
        /// Add another ISessionFactory property for each datastore that your
        /// application uses.
        /// </remarks>
        public static ISessionFactory DefaultSessionFactory
        {
            get
            {
                return _defaultSessionFactory;
            }
        }

        /// <summary>
        /// Initialize the infrastructure for this application
        /// </summary>
        public static void Init()
        {

            //Initialize the default database connection
            _defaultSessionFactory = InitDefaultSessionFactory();

            //Wire up the Infrastructure implementations to the interfaces that are in the Core
            StructureMap.Registry registry = new StructureMap.Registry();
            registry.Scan((scanner) =>
            {
                scanner.AssemblyContainingType<DataAssetContext>();
                scanner.AssemblyContainingType<IDataAssetContext>();
                scanner.AssemblyContainingType<DatasetContext>();
                scanner.AssemblyContainingType<IDatasetContext>();
                scanner.AssemblyContainingType<DataFeedProvider>();
                scanner.AssemblyContainingType<IDataFeedContext>();
                scanner.AssemblyContainingType<MetadataRepositoryProvider>();
                scanner.AssemblyContainingType<IMetadataRepositoryProvider>();
                scanner.AddAllTypesOf<IDataSource>();
                scanner.WithDefaultConventions();
            });

            //Repeat the following line once per database / domain context
            registry.For<IDataAssetContext>().Use(() => new DataAssetContext(_defaultSessionFactory.OpenSession()));
            registry.For<IDatasetContext>().Use((x) => new DatasetContext(_defaultSessionFactory.OpenSession(), x.GetInstance<Lazy<IDataFeatures>>(), x.GetInstance<Lazy<UserService>>() ));

            registry.For<IDataFeedContext>().Use(() => new DataFeedProvider(_defaultSessionFactory.OpenStatelessSession()));
            registry.For<IMetadataRepositoryProvider>().Use(() => new MetadataRepositoryProvider(_defaultSessionFactory.OpenStatelessSession()));
            registry.For<IODCFileProvider>().Use(() => new ODCFileProvider(_defaultSessionFactory.OpenSession()));
            registry.For<IRequestContext>().Use(() => new RequestContext(_defaultSessionFactory.OpenSession()));
            //Register other services
            registry.For<IBaseJobProvider>().AddInstances(x =>
            {
                x.Type<GenericHttpsProvider>().Named(GlobalConstants.DataSoureDiscriminator.HTTPS_SOURCE);
                x.Type<GoogleApiProvider>().Named(GlobalConstants.DataSoureDiscriminator.GOOGLE_API_SOURCE);
                x.Type<DfsDataFlowBasicProvider>().Named(GlobalConstants.DataSoureDiscriminator.DEFAULT_DATAFLOW_DFS_DROP_LOCATION);
                x.Type<FtpDataFlowProvider>().Named(GlobalConstants.DataSoureDiscriminator.FTP_DATAFLOW_SOURCE);
                x.Type<GoogleAPIDataFlowProvider>().Named(GlobalConstants.DataSoureDiscriminator.GOOGLE_API_DATAFLOW_SOURCE);
                x.Type<GenericHttpsDataFlowProvider>().Named(GlobalConstants.DataSoureDiscriminator.GENERIC_HTTPS_DATAFLOW_SOURCE);
            });


            //Register event handlers for MetadataProcessorService
            registry.For<IMessageHandler<string>>().Add<HiveMetadataService>();
            registry.For<IMessageHandler<string>>().Add<SnowflakeEventService>();
            registry.For<IMessageHandler<string>>().Add<SparkConverterEventService>();

            //Wire up Obsidian provider
            Sentry.Web.CachedObsidianUserProvider.ObsidianUserProvider obsidianUserProvider = new Sentry.Web.CachedObsidianUserProvider.ObsidianUserProvider();
            obsidianUserProvider.CacheTimeoutSeconds = int.Parse(Sentry.Configuration.Config.GetHostSetting("ObsidianUserCacheTimeoutMinutes")) * 60;
            //Provide user\pass for possibility of basic auth if necessary
            obsidianUserProvider.Credentials = new System.Net.NetworkCredential(
                Sentry.Configuration.Config.GetHostSetting("ServiceAccountID"),
                Sentry.Configuration.Config.GetHostSetting("ServiceAccountPassword")
                );
            registry.For<Sentry.Web.CachedObsidianUserProvider.IObsidianUserProvider>().Singleton().Use(obsidianUserProvider);
            
            registry.For<IDataFeatures>().Singleton().Use<FeatureFlags.DataFeatures>();
            registry.For<IAssociateInfoProvider>().Singleton().Use<AssociateInfoProvider>();
            registry.For<IExtendedUserInfoProvider>().Singleton().Use<ExtendedUserInfoProvider>();
            registry.For<ISASService>().Singleton().Use<SASServiceProvider>();
            registry.For<IFtpProvider>().Singleton().Use<FtpProvider>();
            registry.For<IS3ServiceProvider>().Singleton().Use<S3ServiceProvider>();
            registry.For<IMessagePublisher>().Singleton().Use<KafkaMessagePublisher>();
            registry.For<IBaseTicketProvider>().Singleton().Use<CherwellProvider>();
            registry.For<RestSharp.IRestClient>().Use(() => new RestSharp.RestClient()).AlwaysUnique();
            registry.For<IInstanceGenerator>().Singleton().Use<ThreadSafeInstanceGenerator>();
            registry.For<IJobScheduler>().Singleton().Use<Sentry.data.Infrastructure.ServiceImplementations.HangfireJobScheduler>();

            ConnectionSettings settings = new ConnectionSettings(new Uri(Configuration.Config.GetHostSetting("ElasticUrl")));
            settings.DefaultMappingFor<ElasticSchemaField>(x => x.IndexName(Configuration.Config.GetHostSetting("ElasticIndexSchemaSearch")));
            settings.BasicAuthentication(Configuration.Config.GetHostSetting("ServiceAccountID"), Configuration.Config.GetHostSetting("ServiceAccountPassword"));
            settings.DefaultMappingFor<DataInventory>(x => x.IndexName(ElasticAliases.DATA_INVENTORY)); //using index alias
            settings.ThrowExceptions();
            registry.For<IElasticClient>().Singleton().Use(new ElasticClient(settings));

            registry.For<IDataInventorySearchProvider>().Add<ElasticDataInventorySearchProvider>().Ctor<IDbExecuter>().Is(new DataInventorySqlExecuter());
            registry.For<IDataInventoryService>().Use<DataInventoryService>();

            // Choose the parameterless constructor.
            registry.For<IBackgroundJobClient>().Singleton().Use<BackgroundJobClient>().SelectConstructor(() => new BackgroundJobClient());
            registry.For<IRecurringJobManager>().Singleton().Use<RecurringJobManager>().SelectConstructor(() => new RecurringJobManager());


            //establish generic httpclient singleton to be used where needed across the application
            var client = new HttpClient(new HttpClientHandler() { UseDefaultCredentials = true });
            registry.For<HttpClient>().Use(client);

            //establish IAssetClient using generic httpClient singleton
            registry.For<IAssetClient>().Singleton().Use<SAIDRestClient.AssetClient>().
                Ctor<HttpClient>().Is(client).
                SetProperty((c) => c.BaseUrl = Sentry.Configuration.Config.GetHostSetting("SaidAssetBaseUrl"));

            //register Quartermaster client
            var qClient = new HttpClient(new HttpClientHandler() { UseDefaultCredentials = true });
            registry.For<Sentry.data.Core.Interfaces.QuartermasterRestClient.IClient>().Singleton().Use<QuartermasterRestClient.Client>().
                Ctor<HttpClient>().Is(qClient).
                SetProperty((c) => c.BaseUrl = Sentry.Configuration.Config.GetHostSetting("QuartermasterServiceBaseUrl"));

            //register Inev client
            var inevClient = new HttpClient(new HttpClientHandler()
            {
                Credentials = new NetworkCredential(
                    Configuration.Config.GetHostSetting("ServiceAccountID"),
                    Configuration.Config.GetHostSetting("ServiceAccountPassword"))
            });
            registry.For<Sentry.data.Core.Interfaces.InfrastructureEventing.IClient>().Singleton().Use<InfrastructureEventing.Client>().
                Ctor<HttpClient>().Is(inevClient).
                SetProperty((c) => c.BaseUrl = Sentry.Configuration.Config.GetHostSetting("InfrastructureEventingServiceBaseUrl"));

            //establish Polly Policy registry
            PolicyRegistry pollyRegistry = new PolicyRegistry();
            registry.For<IReadOnlyPolicyRegistry<string>>().Singleton().Use(pollyRegistry);
            registry.For<IPolicyRegistry<string>>().Singleton().Use(pollyRegistry);

            //register polly policies
            registry.For<IPollyPolicy>().Singleton().Add<ApacheLivyProviderPolicy>();
            registry.For<IPollyPolicy>().Singleton().Add<GoogleApiProviderPolicy>();
            registry.For<IPollyPolicy>().Singleton().Add<GenericHttpProviderPolicy>();
            registry.For<IPollyPolicy>().Singleton().Add<FtpProviderPolicy>();


            //establish httpclient specific to ApacheLivyProvider
            var apacheLivyClient = new HttpClient(new HttpClientHandler() { UseDefaultCredentials = true });
            apacheLivyClient.DefaultRequestHeaders.Accept.Clear();
            apacheLivyClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            apacheLivyClient.DefaultRequestHeaders.Add("X-Requested-By", "data.sentry.com");
            var apacheHttpClientProvider = new HttpClientProvider(apacheLivyClient);
            registry.For<IApacheLivyProvider>().Singleton().Use<ApacheLivyProvider>().
                Ctor<IHttpClientProvider>().Is(apacheHttpClientProvider)
                .SetProperty((c) => c.BaseUrl = Configuration.Config.GetHostSetting("ApacheLivy"));

            //Create the StructureMap container
            _container = new StructureMap.Container(registry);

            // Polly Policy providers
            _container.GetAllInstances<IPollyPolicy>().ToList().ForEach(p => p.Register());


        }

        /// <summary>
        /// Initialize the default database connection, which is used for the Default Session Factory
        /// </summary>
        private static ISessionFactory InitDefaultSessionFactory()
        {

            //Configure the database connection
            global::NHibernate.Cfg.Configuration config = new global::NHibernate.Cfg.Configuration();
            config.DataBaseIntegration((db) =>
            {
                db.ConnectionString = Sentry.Configuration.Config.GetHostSetting("DatabaseConnectionString");
                db.Dialect<MsSql2008Dialect>();
                db.Driver<StackExchange.Profiling.NHibernate.Drivers.MiniProfilerSql2008ClientDriver>();
            });

            //Configure the NHibernate mappings
            ModelMapper modelMapper = new ModelMapper();
            string strDefaultNamespace = typeof(UserMapping).Namespace;
            modelMapper.AddMappings(typeof(UserMapping).Assembly.GetExportedTypes().Where((t) => t.Namespace == strDefaultNamespace));

            //The code below forces all string properties to map to "AnsiString" (aka non-unicode char() or varchar() in SQL Server) for performance reasons
            //TODO: Change so that mappings can use unicode if they really want to
            modelMapper.BeforeMapProperty += (mi, propertyPath, map) =>
            {
                if (propertyPath.LocalMember.GetPropertyOrFieldType() == typeof(string))
                {
                    map.Type(NHibernateUtil.AnsiString);
                }
            };
            config.AddMapping(modelMapper.CompileMappingForAllExplicitlyAddedEntities());

            //Configure the second-level cache
            config.Cache((c) =>
            {
                c.UseQueryCache = true;
                c.Provider<global::NHibernate.Caches.SysCache.SysCacheProvider>();
            });

            //Configure entity Validation
            config.SetInterceptor(new Sentry.NHibernate.ValidationInterceptor());

           return config.BuildSessionFactory();

        }
    }
}
