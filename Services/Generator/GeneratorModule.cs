using Autofac;
using Autofac.Integration.Mef;
using System;

namespace WorkUtilities.Services.Generator
{
	public class GeneratorModule : Module
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
                .RegisterType<EntityGeneratorService>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired();
        }
    }
}
