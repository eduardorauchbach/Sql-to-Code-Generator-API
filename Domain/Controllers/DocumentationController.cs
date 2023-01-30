using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkUtilities.Domain.Models;
using WorkUtilities.Domain.Services.Generator;
using WorkUtilities.Domain.Services.Package;
using WorkUtilities.Models;
using WorkUtilities.Services;

namespace WorkUtilities.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentationController : Controller
    {
        private readonly DocumentationGeneratorService _diagramGeneratorService;

        public DocumentationController(DocumentationGeneratorService diagramGeneratorService)
        {
            _diagramGeneratorService = diagramGeneratorService;
        }

        /// <summary>
        /// Converte GeneratorModel em Mermaid
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
                result = _diagramGeneratorService.ParseFromGenerator(model);

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
