using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WorkUtilities.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkUtilities.Services.Parser;
using WorkUtilities.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WorkUtilities.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class EntryController : ControllerBase
	{
		private readonly EntryParserService _entryParserService;

		public EntryController(EntryParserService entryParserService)
		{
			_entryParserService = entryParserService;
		}

		/// <summary>
		/// Converte SQL em GeneratorModel (Padrão da aplicação)
		/// </summary>
		/// <param name="script">Script de criação de tabelas/banco exportado do SQL</param>
		/// <returns>GeneratorModel</returns>
		[HttpGet]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public IActionResult Get(string script)
		{
			GeneratorModel result;
			ObjectResult response;

			try
			{
				result = new GeneratorModel();
				result.EntryModels = _entryParserService.ParseFromSql(script);

				response = Ok(result);
			}
			catch (Exception ex)
			{
				response = StatusCode(500, ex.Message);
			}

			return response;
		}

		/// <summary>
		/// Converte GeneratorModel (Padrão da aplicação) em SQL
		/// </summary>
		/// <param name="model"></param>
		/// <returns>Script SQL para criação de tabelas, vinculos e indices</returns>
		[HttpPost]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public IActionResult Post(GeneratorModel model)
		{
			string result;
			ObjectResult response;

			try
			{
				result = _entryParserService.ParseToSql(model.EntryModels);

				response = Ok(result);
			}
			catch (Exception ex)
			{
				response = StatusCode(500, ex.Message);
			}

			return response;
		}
	}
}
