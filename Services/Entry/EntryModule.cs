using Autofac;
using Autofac.Integration.Mef;
using System;

namespace WorkUtilities.Services.Entry
{
    public class EntryModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            base.Load(builder);
            builder.RegisterMetadataRegistrationSources();

            _ = builder
                .RegisterType<TSqlParserService>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired();
        }
    }
}
