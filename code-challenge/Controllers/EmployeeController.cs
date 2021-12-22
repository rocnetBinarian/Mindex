using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using challenge.Services;
using challenge.Models;
using challenge.Data;

namespace challenge.Controllers
{
    [Route("api/employee")]
    public class EmployeeController : Controller
    {
        private readonly ILogger _logger;
        private readonly IEmployeeService _employeeService;
        private readonly ICompensationService _compensationService;

        public EmployeeController(ILogger<EmployeeController> logger,
            IEmployeeService employeeService,
            ICompensationService compensationService)
        {
            _logger = logger;
            _employeeService = employeeService;
            _compensationService = compensationService;
        }

        [HttpPost]
        public IActionResult CreateEmployee([FromBody] Employee employee)
        {
            _logger.LogDebug($"Received employee create request for '{employee.FirstName} {employee.LastName}'");

            _employeeService.Create(employee);

            return CreatedAtRoute("getEmployeeById", new { id = employee.EmployeeId }, employee);
        }

        [HttpGet("{id}", Name = "getEmployeeById")]
        public IActionResult GetEmployeeById(String id)
        {
            _logger.LogDebug($"Received employee get request for '{id}'");

            var employee = _employeeService.GetById(id);

            if (employee == null)
                return NotFound();

            return Ok(employee);
        }

        /*
         * WARNING: There is no input checking to confirm newEmployee is valid.  If it's not,
         * the existing will be removed, but no new one will take its place!
         * */
        [HttpPut("{id}")]
        public IActionResult ReplaceEmployee(String id, [FromBody]Employee newEmployee)
        {
            _logger.LogDebug($"Recieved employee update request for '{id}'");

            var existingEmployee = _employeeService.GetById(id);
            if (existingEmployee == null)
                return NotFound();

            _employeeService.Replace(existingEmployee, newEmployee);

            return Ok(newEmployee);
        }

        
        [HttpGet("getReportingStructure/{id}")]
        public IActionResult GetReportingStructure(string id)
        {
            _logger.LogDebug($"Received reporting structure get request for '{id}'");
            var employee = _employeeService.GetById(id);
            if (employee == null)
                return NotFound();

            var rtn = _employeeService.BuildReportingStructure(employee);
            return Ok(rtn);
        }
    
        [HttpGet("getCompensation/{id}")]
        public IActionResult GetCompensation(string id)
        {
            _logger.LogDebug($"Received compensation get request for '{id}'");
            var employee = _employeeService.GetById(id);
            if (employee == null)
                return NotFound();

            try
            {
                var comp = _compensationService.TryGetByEmployeeId(id);
                if (comp == default(Compensation))
                    return NotFound();

                return Ok(comp);
            } catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Bad argument provided when searching for compensation.");
                return BadRequest(ex.Message);
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error when searching for compensation.");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Creates a compensation and associates it to an employee.
        /// 
        /// Another way to do this is to require the json for the Compensation to include a
        /// mostly-empty Employee object (just the id), but that can get messy quickly.
        /// Instead, here, we'll just require an employeeId in the URL, and provide most of the
        /// Compensation data on the post.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="newcomp"></param>
        /// <returns></returns>
        [HttpPost("createCompensation/{id}")]
        public IActionResult CreateCompensation(string id, [FromBody]Compensation newcomp)
        {
            _logger.LogDebug($"Received compensation create request for '{id}'");
            if (newcomp == null || newcomp.effectiveDate == null || newcomp.salary == default(double))
                return BadRequest();

            var employee = _employeeService.GetById(id);
            if (employee == null)
                return NotFound(id);

            newcomp.employee = employee;
            try
            {
                _compensationService.TryCreate(newcomp);
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create compensation.", id);
                return BadRequest(ex.Message);
            }
            return Ok(newcomp);
        }
        

#if DEBUG
        /// <summary>
        /// Had to install the .NET Core 2.1 SDK, so this is just to ensure things are running.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("SanityCheck")]
        public IActionResult SanityCheck()
        {
            _logger.LogDebug("Sanity Check.");
            return Ok("Success");
        }
#endif
    }
}
