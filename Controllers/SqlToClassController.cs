using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkUtilities.Services;

namespace WorkUtilities.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SqlToClassController : Controller
    {
        private readonly SqlToClassService _sqlToClassService;

        public SqlToClassController(SqlToClassService sqlToClassService)
        {
            _sqlToClassService = sqlToClassService;
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
                result = _sqlToClassService.Parse(script);

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
