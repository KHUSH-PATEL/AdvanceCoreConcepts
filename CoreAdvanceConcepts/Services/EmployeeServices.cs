using Azure;
using CoreAdvanceConcepts.Interface;
using CoreAdvanceConcepts.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Dapper.SqlMapper;
using System.Collections.Generic;

namespace CoreAdvanceConcepts.Services
{
    public class EmployeeServices : IEmployeeService
    {
        private readonly IRepository<Employee> _employeeRepository;

        public EmployeeServices(IRepository<Employee> employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public async Task<ResponceMessage<Employee>> CreateEmployeeAsync(Employee employee)
        {
            employee.CreatedDate = DateTime.Now;
            return await _employeeRepository.CreateData(employee);
        }

        public async Task<ResponceMessage<Employee>> DeleteEmployeeAsync(int id)
        {
            ResponceMessage<Employee> response = new ResponceMessage<Employee>();
            try
            {
                ResponceMessage<Employee>? employee = await GetEmployeeByIdAsync(id);
                if (employee == null)
                {
                    response.IsSuccess = false;
                    response.Message = $"Employee with ID {id} not found";
                    return response;
                }
                else
                {
                    employee.Data.FlagDeleted = true;
                    employee.Data.UpdatedDate = DateTime.Now;
                    response = await _employeeRepository.DeleteData(employee.Data);                    
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = "An error occurred while deleting the employee.";
                response.ErrorMessage = new List<string> { ex.Message };
            }
            return response;
        }

        public async Task<ResponceMessage<Employee>> EditEmployeeAsync(int id, Employee employee)
        {
            ResponceMessage<Employee> response = new ResponceMessage<Employee>();
            try
            {
                ResponceMessage<Employee>? newEmployee = await GetEmployeeByIdAsync(id);
                if (id != employee.EmployeeId)
                {
                    response.IsSuccess = false;
                    response.Message = $"{id} and Employee Id: {employee.EmployeeId} does not match";
                    return response;
                }
                else if (employee == null)
                {
                    response.IsSuccess = false;
                    response.Message = $"Employee with {id} not found";
                }
                else
                {
                    newEmployee.Data.UpdatedDate = DateTime.Now;
                    newEmployee.Data.BirthDate = employee.BirthDate;
                    newEmployee.Data.FullName = employee.FullName;
                    newEmployee.Data.City = employee.City;
                    newEmployee.Data.Email = employee.Email;
                    newEmployee.Data.PhoneNumber = employee.PhoneNumber;
                    response = await _employeeRepository.EditData(newEmployee.Data);
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = "An error occurred while deleting the employee.";
                response.ErrorMessage = new List<string> { ex.Message };
            }
            return response;
        }

        public async Task<ResponceMessage<Employee>> GetEmployeeByIdAsync(int id)
        {
            ResponceMessage<Employee> response = new ResponceMessage<Employee>();
            var employeeList = await _employeeRepository.GetDataList();
            try
            {
                var employee = employeeList.Data.FirstOrDefault(x => !x.FlagDeleted && x.EmployeeId == id);
                response.IsSuccess = true;
                response.Data = employee;
                if (employee == null)
                {
                    response.IsSuccess = false;
                    response.Message = $"No employee found with Id: {id}";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = "An error occurred while fetching entities.";
                response.ErrorMessage = new List<string> { ex.Message };
            }
            return response;

        }

        public async Task<ResponceMessage<IEnumerable<Employee>>> GetEmployeesAsync()
        {
            ResponceMessage<IEnumerable<Employee>> response = new ResponceMessage<IEnumerable<Employee>> ();
            var employeeList = await _employeeRepository.GetDataList();
            try
            {
                var employee = employeeList.Data.Where(x => !x.FlagDeleted).ToList();
                response.IsSuccess = true;
                response.Data = employee;
                response.DataCount = employee.Count;
                if (employee == null)
                {
                    response.IsSuccess = false;
                    response.Message = $"No employees found";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = "An error occurred while fetching entities.";
                response.ErrorMessage = new List<string> { ex.Message };
            }
            return response;
        }
    }
}
