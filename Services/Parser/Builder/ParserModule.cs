using Autofac;
using Autofac.Integration.Mef;
using System;

namespace WorkUtilities.Services.Parser.Builder
{
    public class ParserModule : Module
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
                .RegisterType<EntryParserService>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired();
        }
    }
}
