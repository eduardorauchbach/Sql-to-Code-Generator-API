using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WorkUtilities.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkUtilities.Models;
using WorkUtilities.Domain.Services.Generator;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WorkUtilities.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EntityFrameworkController : ControllerBase
    {
        private readonly EntityGeneratorService _entityGeneratorService;

        public EntityFrameworkController(EntityGeneratorService entityGeneratorService)
        {
            _entityGeneratorService = entityGeneratorService;
        }

        /// <summary>
        /// Converte GeneratorModel em lista de "Entity Builders"
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
                result = string.Join("\n----------------------------------------\n\n", _entityGeneratorService.ParseFromGenerator(model));

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
