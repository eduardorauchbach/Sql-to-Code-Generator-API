using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkUtilities.Helpers;

namespace WorkUtilities.Domain.Services.Generator
{
    public class ModuleGeneratorService
    {
        public string BuildModule(string projectName, string zoneName, List<string> types)
        {
            StringBuilder result;
            int tab;

            try
            {
                result = new StringBuilder();
                tab = 0;

                result.AppendCode(tab, "using Autofac;", 1);
                result.AppendCode(tab, "using Autofac.Integration.Mef;", 1); 
                result.AppendCode(tab, "using System;", 2);

                result.AppendCode(tab, $"namespace {projectName}.Domain.Repository.Builder", 1);
                result.AppendCode(tab, "{", 1);
                tab++;
                {
                    result.AppendCode(tab, $"public class {zoneName}Module", 1);
                    result.AppendCode(tab, "{", 1);
                    tab++;
                    {
                        result.AppendCode(tab, $"protected override void Load(ContainerBuilder builder)", 1);
                        result.AppendCode(tab, "{", 1);
                        tab++;
                        {
                            result.AppendCode(tab, "if (builder is null)", 1);
                            result.AppendCode(tab, "{", 1);
                            tab++;
                            {
                                result.AppendCode(tab, "throw new ArgumentNullException(nameof(builder));", 1);
                            }
                            tab--;
                            result.AppendCode(tab, "}", 2);

                            result.AppendCode(tab, "base.Load(builder);", 1);
                            result.AppendCode(tab, "builder.RegisterMetadataRegistrationSources();", 2);

                            foreach (string t in types)
                            {
                                result.AppendCode(tab, "_ = builder", 1);
                                tab++;

                                result.AppendCode(tab, $".RegisterType<{t}>()", 1);
                                result.AppendCode(tab, $".InstancePerLifetimeScope()", 1);
                                result.AppendCode(tab, $".PropertiesAutowired();", 2);

                                tab--;
                            }
                        }
                        tab--;
                        result.AppendCode(tab, "}", 1);
                    }
                    tab--;
                    result.AppendCode(tab, "}", 2);
                }
                tab--;
                result.AppendCode(tab, "}", 1);
            }
            catch
            {
                throw;
            }

            return result.ToString();
        }
    }
}
