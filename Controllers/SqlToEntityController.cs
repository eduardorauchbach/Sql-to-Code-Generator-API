using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WorkUtilities.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WorkUtilities.Controllers
{    
    [Route("api/[controller]")]
    [ApiController]
    public class SqlToEntityController : ControllerBase
    {
        private readonly SqlToEntityService _sqlToEntityService;

        public SqlToEntityController(SqlToEntityService sqlToEntityService)
        {
            _sqlToEntityService = sqlToEntityService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Get(string script)
        {
            string result;
            ObjectResult response;

            try
            {
                result = _sqlToEntityService.Parse(script);

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
