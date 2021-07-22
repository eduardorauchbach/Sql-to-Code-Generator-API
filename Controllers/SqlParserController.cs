using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WorkUtilities.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkUtilities.Services.Entry;
using WorkUtilities.Model;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WorkUtilities.Controllers
{    
    [Route("api/[controller]")]
    [ApiController]
    public class SqlParserController : ControllerBase
    {
        private readonly TSqlParserService _sqlTranslatorService;
        private readonly EntryModelValidator _validations;

        public SqlParserController(TSqlParserService sqlTranslatorService)
        {
            _sqlTranslatorService = sqlTranslatorService;
            _validations = new EntryModelValidator();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Post(string script)
        {
            List<EntryModel> result;
            ObjectResult response;

            try
            {
                result = _sqlTranslatorService.Translate(script);

                response = Ok(result);
            }
            catch (Exception ex)
            {
                response = StatusCode(500, ex.Message);
            }

            return response;
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Put(EntryModel model)
        {
            List<EntryModel> result;
            ObjectResult response;

            try
            {
                //result = _sqlTranslatorService.Translate(script);

                response = Ok(null);
            }
            catch (Exception ex)
            {
                response = StatusCode(500, ex.Message);
            }

            return response;
        }
    }
}
