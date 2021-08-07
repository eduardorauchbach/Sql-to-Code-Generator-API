using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkUtilities.Models;
using WorkUtilities.Services;
using WorkUtilities.Services.Generator;

namespace WorkUtilities.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RepositoryController : Controller
    {
        private readonly RepositoryGeneratorService _repositoryGeneratorService;

        public RepositoryController(RepositoryGeneratorService repositoryGeneratorService)
        {
            _repositoryGeneratorService = repositoryGeneratorService;
        }

        /// <summary>
        /// Converte GeneratorModel em lista de "Repositories" com ações de CRUD de acordo com os relacionamentos
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Lista contendo todos os builders envolvidos separados por traços</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Post(GeneratorModel model)
        {
            string result;
            ObjectResult response;

            try
            {
                result = string.Join("\n----------------------------------------\n\n", _repositoryGeneratorService.ParseFromGenerator(model));

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
