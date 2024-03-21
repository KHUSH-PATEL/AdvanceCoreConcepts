using CoreAdvanceConcepts.Controllers;
using CoreAdvanceConcepts.Interface;
using CoreAdvanceConcepts.Models;
using CoreAdvanceConcepts.Services;
using Microsoft.AspNetCore.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Linq.Expressions;

namespace CoreAdvanceConcepts.MsUnitTest.Services
{
    [TestClass]
    public class EmployeeServiceTest
    {
        private Mock<IRepository<Employee>> mockEmployeeRepository;
        private EmployeeServices employeeServices;
        private List<Employee> employeeList = new List<Employee>();

        [TestInitialize]
        public void Initialize()
        {
            mockEmployeeRepository = new Mock<IRepository<Employee>>();
            employeeServices = new EmployeeServices(mockEmployeeRepository.Object);
            employeeList = GetTestEmployees();
        }

        [TestMethod]
        public async Task GetEmployeesAsync_ReturnsOkResult()
        {
            // Arrange
            var response = new ResponceMessage<IEnumerable<Employee>>
            {
                IsSuccess = true,
                Data = employeeList
            };
            mockEmployeeRepository.Setup(setup => setup.GetDataList())
                                  .ReturnsAsync(response);
            var employees = response.Data.Where(x => !x.FlagDeleted).ToList();

            // Act
            var serviceResult = await employeeServices.GetEmployeesAsync();

            // Assert
            Assert.IsTrue(serviceResult.IsSuccess);
            Assert.IsNotNull(serviceResult.Data);
            Assert.AreEqual(employees.Count, serviceResult.DataCount);
            mockEmployeeRepository.Verify(x => x.GetDataList(), Times.Once);
        }

        [TestMethod]
        public async Task GetEmployeesAsyncWhenNoEmployeeFoundReturnsOkResult()
        {
            // Arrange
            var employees = new List<Employee>();
            var message = "No employees found";
            var response = new ResponceMessage<IEnumerable<Employee>>
            {
                IsSuccess = true,
                Data = employees
            };
            mockEmployeeRepository.Setup(setup => setup.GetDataList())
                                  .ReturnsAsync(response);

            // Act
            var serviceResult = await employeeServices.GetEmployeesAsync();

            // Assert
            Assert.IsTrue(serviceResult.IsSuccess);
            Assert.IsNull(serviceResult.Data);
            Assert.AreEqual(message, serviceResult.Message);
            mockEmployeeRepository.Verify(x => x.GetDataList(), Times.Once);
        }

        [TestMethod]
        public async Task GetEmployeesAsyncWhenFailureReturnResult()
        {
            // Arrange
            var errorMessage = "Test error message";
            string responseMessage = "An error occurred while fetching entities.";
            mockEmployeeRepository.Setup(x => x.GetDataList()).ThrowsAsync(new Exception(errorMessage));

            // Act
            var serviceResult = await employeeServices.GetEmployeesAsync();

            // Assert
            Assert.IsFalse(serviceResult.IsSuccess);
            Assert.IsNull(serviceResult.Data);
            Assert.AreEqual(responseMessage, serviceResult.Message);
            Assert.IsNotNull(serviceResult.ErrorMessage);
            Assert.AreEqual(1, serviceResult.ErrorMessage.Count);
            Assert.AreEqual(errorMessage, serviceResult.ErrorMessage[0]);
            mockEmployeeRepository.Verify(x => x.GetDataList(), Times.Once);
        }

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(5)]
        [DataRow(7)]
        public async Task GetEmployeeByIdAsync_WhenReturnsOkResult(int id)
        {
            // Arrange
            var employee = employeeList.FirstOrDefault(x => x.EmployeeId == id);
            var response = new ResponceMessage<Employee>
            {
                IsSuccess = true,
                Data = employee
            };            
            mockEmployeeRepository.Setup(repo => repo.GetDataById(It.IsAny<Expression<Func<Employee, bool>>>()))
                                  .ReturnsAsync(response);

            // Act
            var serviceResult = await employeeServices.GetEmployeeByIdAsync(id);

            // Assert
            Assert.IsTrue(serviceResult.IsSuccess);
            Assert.IsNotNull(serviceResult.Data);
            Assert.IsFalse(serviceResult.Data.FlagDeleted);
            mockEmployeeRepository.Verify(x => x.GetDataById(It.IsAny<Expression<Func<Employee, bool>>>()), Times.Once);
        }

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(5)]
        [DataRow(7)]
        public async Task GetEmployeeByIdAsync_WhenReturnsFailureResult(int id)
        {
            // Arrange
            var errorMessage = "Test error message";
            string responseMessage = "An error occurred while fetching entities.";
            mockEmployeeRepository.Setup(repo => repo.GetDataById(It.IsAny<Expression<Func<Employee, bool>>>()))
                                  .ThrowsAsync(new Exception(errorMessage));

            // Act
            var result = await employeeServices.GetEmployeeByIdAsync(id);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNull(result.Data);
            Assert.AreEqual(responseMessage, result.Message);
            Assert.IsNotNull(result.ErrorMessage);
            Assert.AreEqual(errorMessage, result.ErrorMessage[0]);
        }

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(5)]
        [DataRow(7)]
        public async Task DeleteEmployeeAsync_WhenEmployeeExists(int id)
        {
            // Arrange
            var employee = employeeList.FirstOrDefault(x => x.EmployeeId == id);
            var response = new ResponceMessage<Employee>
            {
                IsSuccess = true,
                Data = employee
            };

            mockEmployeeRepository.Setup(x => x.GetDataById(It.IsAny<Expression<Func<Employee, bool>>>()))
                                  .ReturnsAsync(response);
            mockEmployeeRepository.Setup(x => x.DeleteData(employee)).ReturnsAsync(response);

            // Act
            var serviceResult = await employeeServices.DeleteEmployeeAsync(id);

            // Assert
            Assert.IsNotNull(serviceResult);
            Assert.IsNotNull(serviceResult.Data);
            Assert.IsTrue(serviceResult.IsSuccess);
            Assert.AreEqual(string.Empty, serviceResult.Message);
            Assert.IsTrue(serviceResult.Data.FlagDeleted);
            Assert.IsTrue(serviceResult.Data.UpdatedDate > DateTime.MinValue);
            mockEmployeeRepository.Verify(x => x.DeleteData(employee), Times.Once);
        }

        [DataTestMethod]
        [DataRow(2)]
        [DataRow(4)]
        [DataRow(6)]
        public async Task DeleteEmployeeAsync_WhenEmployeeNotExists(int id)
        {
            // Arrange
            string responseMessage = $"Employee with ID {id} not found";
            var employee = employeeList.FirstOrDefault(x => x.EmployeeId == id);
            var response = new ResponceMessage<Employee>
            {
                IsSuccess = true,
                Data = employee,
                Message = responseMessage
            };

            mockEmployeeRepository.Setup(x => x.GetDataById(It.IsAny<Expression<Func<Employee, bool>>>()))
                                  .ReturnsAsync(response);

            // Act
            var serviceResult = await employeeServices.DeleteEmployeeAsync(id);

            // Assert
            Assert.IsNull(serviceResult.Data);
            Assert.IsTrue(response.IsSuccess);
            Assert.AreEqual(responseMessage, response.Message);
            mockEmployeeRepository.Verify(x => x.GetDataById(It.IsAny<Expression<Func<Employee, bool>>>()), Times.Once);
        }

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(3)]
        [DataRow(10)]
        public async Task EditEmployeeAsync_WhenIdDoesNotMatchEmployeeId_ReturnsErrorMessage(int id)
        {
            // Arrange
            Employee employee = new Employee
            {
                EmployeeId = 2,
                FullName = "John Doe"
            };
            string responseMessage = $"{id} and Employee Id: {employee.EmployeeId} does not match";

            // Act
            var result = await employeeServices.EditEmployeeAsync(id, employee);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(responseMessage, result.Message);
            mockEmployeeRepository.Verify(x => x.GetDataById(It.IsAny<Expression<Func<Employee, bool>>>()), Times.Once);
        }

        [TestMethod]
        public async Task EditEmployeeAsync_WhenFlagDeletedIsTrue_ReturnsErrorMessage()
        {
            // Arrange
            int id = 2;
            string responseMessage = $"Employee with ID {id} not found";
            Employee employee = new Employee
            {
                EmployeeId = 2,
                FullName = "John Doe",
            };
            var response = new ResponceMessage<Employee>
            {
                IsSuccess = true,
                Data = employeeList.FirstOrDefault(x => x.EmployeeId == 2),
                Message = responseMessage
            };

            mockEmployeeRepository.Setup(x => x.GetDataById(It.IsAny<Expression<Func<Employee, bool>>>()))
                                   .ReturnsAsync(response);

            // Act
            var result = await employeeServices.EditEmployeeAsync(id, employee);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(responseMessage, result.Message);
            mockEmployeeRepository.Verify(x => x.GetDataById(It.IsAny<Expression<Func<Employee, bool>>>()), Times.Once);
            mockEmployeeRepository.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task EditEmployeeAsync_WhenSuccessfullyEditsData_ReturnsSuccessMessage()
        {
            // Arrange
            int id = 1;
            Employee employee = new Employee
            {
                EmployeeId = id,
                FullName = "John Doe",
            };
            var employeeResponse = new ResponceMessage<Employee>
            {
                IsSuccess = true,
                Data = employeeList.FirstOrDefault(x => x.EmployeeId == id)
            };
            var response = new ResponceMessage<Employee>
            {
                IsSuccess = true,
                Data = employee
            };

            mockEmployeeRepository.Setup(x => x.GetDataById(It.IsAny<Expression<Func<Employee, bool>>>()))
                                   .ReturnsAsync(employeeResponse);
            mockEmployeeRepository.Setup(x => x.EditData(It.IsAny<Employee>()))
                                   .ReturnsAsync(response);

            // Act
            var result = await employeeServices.EditEmployeeAsync(id, employee);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(employee.FullName, response.Data.FullName);
            mockEmployeeRepository.Verify(x => x.GetDataById(It.IsAny<Expression<Func<Employee, bool>>>()), Times.Once);
            mockEmployeeRepository.Verify(x => x.EditData(It.IsAny<Employee>()), Times.Once);
            mockEmployeeRepository.VerifyNoOtherCalls();
        }

        [TestCleanup]
        public void Cleanup()
        {
            mockEmployeeRepository.Reset();
        }

        public List<Employee> GetTestEmployees()
        {
            return new List<Employee>
            {
                new Employee { EmployeeId = 1, FullName = "Khush Makadiya", City = "Rajkot", PhoneNumber = "9898989898", Email = "mk3@gmail.com", FlagDeleted = false },
                new Employee { EmployeeId = 2, FullName = "Jenish Raiyani", City = "Junagadh", PhoneNumber = "8527418528", Email = "JR3@gmail.com", FlagDeleted = true },
                new Employee { EmployeeId = 3, FullName = "Akash Rana", City = "Chonga", PhoneNumber = "8989895623", Email = "AR3@gmail.com", FlagDeleted = false },
                new Employee { EmployeeId = 4, FullName = "Some Other Name", City = "Some City", PhoneNumber = "1234567890", Email = "example@gmail.com", FlagDeleted = true },
                new Employee { EmployeeId = 5, FullName = "Another Name", City = "Another City", PhoneNumber = "9876543210", Email = "another@example.com", FlagDeleted = false },
                new Employee { EmployeeId = 6, FullName = "Test Person", City = "Test City", PhoneNumber = "1112223333", Email = "test@example.com", FlagDeleted = true },
                new Employee { EmployeeId = 7, FullName = "John Doe", City = "New York", PhoneNumber = "5555555555", Email = "john@example.com", FlagDeleted = false },
                new Employee { EmployeeId = 8, FullName = "Jane Smith", City = "Los Angeles", PhoneNumber = "4444444444", Email = "jane@example.com", FlagDeleted = true},
                new Employee { EmployeeId = 9, FullName = "Alice Johnson", City = "Chicago", PhoneNumber = "3333333333", Email = "alice@example.com", FlagDeleted = false },
                new Employee { EmployeeId = 10, FullName = "Bob Brown", City = "Houston", PhoneNumber = "2222222222", Email = "bob@example.com", FlagDeleted = false }
            };
        }
    }
}
