using CoreAdvanceConcepts.Models;
using Microsoft.AspNetCore.Mvc;

namespace CoreAdvanceConcepts.Interface
{
    public interface IEmployeeService
    {
        Task<ResponceMessage<IEnumerable<Employee>>> GetEmployeesAsync();
        Task<ResponceMessage<Employee>> GetEmployeeByIdAsync(int id);
        Task<ResponceMessage<Employee>> EditEmployeeAsync(int id, Employee employee);
        Task<ResponceMessage<Employee>> CreateEmployeeAsync(Employee employee);
        Task<ResponceMessage<Employee>> DeleteEmployeeAsync(int id);
    }
}
