using Autofac;
using Autofac.Integration.Mef;
using System;

namespace WorkUtilities.Domain.Services.Package.Builder
{
    public class PackagerModule : Module
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
                .RegisterType<FilePackagerService>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired();
        }
    }
}
