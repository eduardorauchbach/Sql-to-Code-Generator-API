﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WorkUtilities.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkUtilities.Services.Translation;
using WorkUtilities.Model;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WorkUtilities.Controllers
{    
    [Route("api/[controller]")]
    [ApiController]
    public class SqlParserController : ControllerBase
    {
        private readonly TSqlTranslatorService _sqlTranslatorService;

        public SqlParserController(TSqlTranslatorService sqlTranslatorService)
        {
            _sqlTranslatorService = sqlTranslatorService;
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
    }
}
