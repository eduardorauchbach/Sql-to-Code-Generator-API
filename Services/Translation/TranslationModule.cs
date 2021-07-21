using Autofac;
using Autofac.Integration.Mef;
using System;

namespace WorkUtilities.Services.Translation
{
    public class TranslationModule : Module
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
                .RegisterType<TSqlTranslatorService>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired();
        }
    }
}
