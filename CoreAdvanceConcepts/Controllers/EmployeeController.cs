using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoreAdvanceConcepts.DataContext;
using CoreAdvanceConcepts.Models;
using CoreAdvanceConcepts.Interface;
using CoreAdvanceConcepts.Services;
using CoreAdvanceConcepts.Caching;
using Asp.Versioning;

namespace CoreAdvanceConcepts.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;        

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;            
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
        {
            var responce = await _employeeService.GetEmployeesAsync();
            if (responce.IsSuccess)
                return Ok(responce);
            else
                return BadRequest(responce);
        }
      
        [HttpGet("{id}")]
        [ApiVersion("2.0")]
        public async Task<ActionResult<Employee>> GetEmployeeById(int id)
        {
            var responce = await _employeeService.GetEmployeeByIdAsync(id);

            if (responce.IsSuccess)
                return Ok(responce);
            else
                return NotFound(responce);
        }

        [HttpPut]
        public async Task<ActionResult<Employee>> EditEmployee(int id, Employee employee)
        {
            var responce = await _employeeService.EditEmployeeAsync(id, employee);
            if (responce.IsSuccess)
            {
                return Ok(responce);
            }
            else
                return BadRequest(responce);
        }

        [HttpPost]
        public async Task<ActionResult<Employee>> CreateEmployee(Employee employee)
        {
            var responce = await _employeeService.CreateEmployeeAsync(employee);
            if (responce.IsSuccess)
            {
                return CreatedAtAction("CreateEmployee", responce);
            }
            else
                return BadRequest(responce);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var responce = await _employeeService.DeleteEmployeeAsync(id);
            if (responce.IsSuccess)
            {
                return Ok(responce);
            }
            else
                return BadRequest(responce);
        }
    }
}
