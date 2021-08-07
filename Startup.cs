using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using WorkUtilities.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.FileProviders;

namespace WorkUtilities
{
	public class Startup
	{
		private const string SwaggerTitle = "Code Generator";
		private const string SwaggerDescription = "Aplicação Demo";
		private const string SwaggerVersion = "v1.0";

		private readonly string _caminhoAplicacao;
		private readonly string _nomeAplicacao;

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;

			_caminhoAplicacao = AppDomain.CurrentDomain.BaseDirectory;
			_nomeAplicacao = AppDomain.CurrentDomain.FriendlyName;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			_ = services.AddControllers();
			_ = services.AddResponseCompression();
			_ = services.AddFluentValidation(fvc => fvc.RegisterValidatorsFromAssemblyContaining<Startup>());
			_ = services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc(SwaggerVersion, new OpenApiInfo
				{
					Title = SwaggerTitle,
					Description = SwaggerDescription,
					Version = SwaggerVersion,
					Contact = new OpenApiContact
					{
						Name = "Eduardo Rauchbach",
						Email = "eduardo.rauchbach@gmail.com",
					},
				});
				c.UseAllOfForInheritance();
				c.UseOneOfForPolymorphism();

				string caminhoXmlDoc = Path.Combine(_caminhoAplicacao, $"{_nomeAplicacao}.xml");

				c.IncludeXmlComments(caminhoXmlDoc);
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			string endpoint = $"{SwaggerVersion}/swagger.json";
			string title = $"{SwaggerTitle} {SwaggerVersion}";

			_ = app.UseDeveloperExceptionPage();

			_ = app.UseStaticFiles();
			_ = app.UseStaticFiles(
				new StaticFileOptions
				{
					FileProvider = new PhysicalFileProvider(_caminhoAplicacao + "/Content"),
					RequestPath = "/local/Content"
				}
			);

			_ = app.UseSwagger();
			_ = app.UseSwaggerUI(c =>
				{
					c.SwaggerEndpoint(endpoint, title);
					c.InjectStylesheet($"/local/Content/{_nomeAplicacao}.css");
				}
			);

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}
