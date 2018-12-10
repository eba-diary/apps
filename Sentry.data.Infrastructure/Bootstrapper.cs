﻿using System.Linq;
using NHibernate;
using NHibernate.Mapping.ByCode;
using Sentry.data.Core;
using NHibernate.Dialect;
using NHibernate.Cfg;
using Sentry.data.Infrastructure.Mappings.Primary;

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
                scanner.AssemblyContainingType<dataAssetContext>();
                scanner.AssemblyContainingType<IDataAssetContext>();
                scanner.AssemblyContainingType<datasetContext>();
                scanner.AssemblyContainingType<IDatasetContext>();
                scanner.AssemblyContainingType<DataFeedProvider>();
                scanner.AssemblyContainingType<IDataFeedContext>();
                scanner.AssemblyContainingType<MetadataRepositoryProvider>();
                scanner.AssemblyContainingType<IMetadataRepositoryProvider>();
                scanner.AddAllTypesOf<IDataSource>();
                scanner.WithDefaultConventions();
            });

            //Repeat the following line once per database / domain context
            registry.For<IDataAssetContext>().Use(() => new dataAssetContext(_defaultSessionFactory.OpenSession()));
            registry.For<IDatasetContext>().Use(() => new datasetContext(_defaultSessionFactory.OpenSession()));
            registry.For<IDataFeedContext>().Use(() => new DataFeedProvider(_defaultSessionFactory.OpenStatelessSession()));
            registry.For<IMetadataRepositoryProvider>().Use(() => new MetadataRepositoryProvider(_defaultSessionFactory.OpenStatelessSession()));
            registry.For<IODCFileProvider>().Use(() => new ODCFileProvider(_defaultSessionFactory.OpenSession()));
            registry.For<IRequestContext>().Use(() => new RequestContext(_defaultSessionFactory.OpenSession()));
            registry.For<IReportContext>().Use(() => new ReportContext(_defaultSessionFactory.OpenSession()));

            //Register other services
            Sentry.Web.CachedObsidianUserProvider.ObsidianUserProvider obsidianUserProvider = new Sentry.Web.CachedObsidianUserProvider.ObsidianUserProvider();
            obsidianUserProvider.CacheTimeoutSeconds = int.Parse(Sentry.Configuration.Config.GetHostSetting("ObsidianUserCacheTimeoutMinutes")) * 60;
            registry.For<Sentry.Web.CachedObsidianUserProvider.IObsidianUserProvider>().Singleton().Use(obsidianUserProvider);
            registry.For<IAssociateInfoProvider>().Singleton().Use<AssociateInfoProvider>();
            registry.For<IExtendedUserInfoProvider>().Singleton().Use<ExtendedUserInfoProvider>();
            registry.For<ISASService>().Singleton().Use<SASServiceProvider>();
            registry.For<IFtpProvider>().Singleton().Use<FtpProvider>();
            registry.For<IDatasetService>().Singleton().Use<S3ServiceProvider>();
            registry.For<IMessagePublisher>().Singleton().Use<KafkaMessagePublisher>();
            
            //Create the StructureMap container
            _container = new StructureMap.Container(registry);

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
