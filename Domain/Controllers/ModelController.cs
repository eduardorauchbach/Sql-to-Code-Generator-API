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
    public class ModelController : Controller
    {
        private readonly ModelGeneratorService _modelGeneratorService;
        private readonly FilePackagerService _filePackagerService;

        public ModelController(ModelGeneratorService modelGeneratorService, FilePackagerService filePackagerService)
        {
            _modelGeneratorService = modelGeneratorService;
            _filePackagerService = filePackagerService;
        }

        /// <summary>
        /// Converte GeneratorModel em lista de "Models" com validação
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
                result = string.Join("\n----------------------------------------\n\n", _modelGeneratorService.ParseFromGenerator(model).Select(x=>x.ContentText));

                response = Ok(result);
            }
            catch (Exception ex)
            {
                response = StatusCode(500, ex.Message);
            }

            return response;
        }

        [HttpPost("download")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public FileContentResult PostDownload(GeneratorModel model)
        {
            List<InMemoryFile> memoryFiles;
            FileContentResult result;

            try
            {
                memoryFiles = _modelGeneratorService.ParseFromGenerator(model);
                result = new FileContentResult(_filePackagerService.BuildPackage(memoryFiles), "application/zip")
                {
                    FileDownloadName = "Models"
                };
            }
            catch
            {
                throw;
            }

            return result;
        }
    }
}
