using Azure;
using CoreAdvanceConcepts.Models;
using Moq;
using MySqlX.XDevAPI.Common;
using Org.BouncyCastle.Ocsp;
using System.Linq.Expressions;
using Xunit.Sdk;

namespace CoreAdvanceConcepts.Test.Services
{
    public class EmployeeServicesTest
    {
        private readonly Mock<IRepository<Employee>> mockEmployeeRepository;
        private readonly EmployeeServices employeeServices;
        public EmployeeServicesTest()
        {
            mockEmployeeRepository = new Mock<IRepository<Employee>>();
            employeeServices = new EmployeeServices(mockEmployeeRepository.Object);
        }
        
        [Fact]
        public async Task GetEmployeesAsync_Employee_ReturnsOkResult()
        {
            // Arrange
            var employees = GetTestEmployees();
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
            Assert.True(serviceResult.IsSuccess);
            var employeeList = response.Data.Where(x => !x.FlagDeleted).ToList();
            Assert.NotNull(serviceResult.Data);
            Assert.Equal(employeeList.Count, serviceResult.DataCount);
            Assert.Equal(employeeList, serviceResult.Data);
            mockEmployeeRepository.Verify(x => x.GetDataList(), Times.Once);
        }
        [Fact]
        public async Task GetEmployeesAsync_EmployeeNullList_ReturnsOkResult()
        {
            // Arrange
            var employees = new List<Employee>();
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
            Assert.True(serviceResult.IsSuccess);

            Assert.Null(serviceResult.Data);
            Assert.Equal("No employees found", serviceResult.Message);
            mockEmployeeRepository.Verify(x => x.GetDataList(), Times.Once);
        }

        [Fact]
        public async Task GetEmployeesAsync_Employee_FailureResult()
        {
            // Arrange
            var errorMessage = "Test error message";
            string responceMessage = "An error occurred while fetching entities.";
            mockEmployeeRepository.Setup(x => x.GetDataList()).ThrowsAsync(new Exception(errorMessage));

            // Act
            var serviceResult = await employeeServices.GetEmployeesAsync();

            // Assert
            Assert.False(serviceResult.IsSuccess);
            Assert.Null(serviceResult.Data);
            Assert.Equal(responceMessage, serviceResult.Message);
            Assert.NotNull(serviceResult.ErrorMessage);
            Assert.Single(serviceResult.ErrorMessage);
            Assert.Equal(errorMessage, serviceResult.ErrorMessage[0]);
            mockEmployeeRepository.Verify(x => x.GetDataList(), Times.Once);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(7)]
        public async Task GetEmployeeByIdAsync_Employee_ReturnsOkResult(int id)
        {
            var employees = GetTestEmployees().FirstOrDefault(x => x.EmployeeId == id);
            var response = new ResponceMessage<Employee>
            {
                IsSuccess = true,
                Data = employees
            };

            //Arrange
            mockEmployeeRepository.Setup(repo => repo.GetDataById(It.IsAny<Expression<Func<Employee, bool>>>()))
                  .ReturnsAsync(response);

            //Act
            var serviceResult = await employeeServices.GetEmployeeByIdAsync(id);

            // Assert
            Assert.True(serviceResult.IsSuccess);
            Assert.NotNull(serviceResult.Data);
            Assert.False(serviceResult.Data.FlagDeleted);
            mockEmployeeRepository.Verify(x => x.GetDataById(It.IsAny<Expression<Func<Employee, bool>>>()), Times.Once);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(7)]
        public async Task GetEmployeeByIdAsync_Employee_ReturnsFailureResult(int id)
        {
            // Arrange
            var errorMessage = "Test error message";
            string responceMessage = "An error occurred while fetching entities.";
            mockEmployeeRepository.Setup(repo => repo.GetDataById(It.IsAny<Expression<Func<Employee, bool>>>()))
                                            .ThrowsAsync(new Exception(errorMessage));

            // Act
            var result = await employeeServices.GetEmployeeByIdAsync(id);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Null(result.Data);
            Assert.Equal(responceMessage, result.Message);
            Assert.NotNull(result.ErrorMessage);
            Assert.Equal(errorMessage, result?.ErrorMessage[0]);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(7)]
        public async Task DeleteEmployeeAsync_DeletesEmployee_WhenEmployeeExists(int id)
        {
            // Arrange
            var employee = GetTestEmployees().FirstOrDefault(x => x.EmployeeId == id);
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
            Assert.NotNull(serviceResult.Data);
            Assert.True(response.IsSuccess);
            Assert.Equal(string.Empty ,response.Message);
            Assert.True(serviceResult.Data.FlagDeleted);
            Assert.True(serviceResult.Data.UpdatedDate > DateTime.MinValue);
            mockEmployeeRepository.Verify(x => x.DeleteData(serviceResult.Data), Times.Once);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(4)]
        [InlineData(6)]
        public async Task DeleteEmployeeAsync_DeletesEmployee_WhenEmployeeNotExists(int id)
        {
            // Arrange
            string responceMessage = $"Employee with ID {id} not found";
            var employee = GetTestEmployees().FirstOrDefault(x => x.EmployeeId == id);
            var response = new ResponceMessage<Employee>
            {
                IsSuccess = true,
                Data = employee,
                Message = responceMessage
            };

            mockEmployeeRepository.Setup(x => x.GetDataById(It.IsAny<Expression<Func<Employee, bool>>>()))
                .ReturnsAsync(response);

            // Act
            var serviceResult = await employeeServices.DeleteEmployeeAsync(id);

            // Assert
            Assert.Null(serviceResult.Data);
            Assert.True(response.IsSuccess);
            Assert.Equal(responceMessage, response.Message);
            mockEmployeeRepository.Verify(x => x.GetDataById(It.IsAny<Expression<Func<Employee, bool>>>()), Times.Once);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(10)]
        public async Task EditEmployeeAsync_IdDoesNotMatchEmployeeId_ReturnsErrorMessage(int id)
        {
            // Arrange
            Employee employee = new Employee
            {
                EmployeeId = 2,
                FullName = "John Doe"                
            };
            string responceMessage = $"{id} and Employee Id: {employee.EmployeeId} does not match";
            // Act
            var result = await employeeServices.EditEmployeeAsync(id, employee);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(responceMessage, result.Message);
            mockEmployeeRepository.Verify(x => x.GetDataById(It.IsAny<Expression<Func<Employee, bool>>>()), Times.Once);
        }

        [Fact]
        public async Task EditEmployeeAsync_FlagDeletedIsTrue_ReturnsErrorMessage()
        {
            // Arrange
            int id = 2;
            string responceMessage = $"Employee with ID {id} not found";
            Employee employee = new Employee
            {
                EmployeeId = 2,
                FullName = "John Doe",
            };
            var response = new ResponceMessage<Employee>
            {
                IsSuccess = true,
                Data = GetTestEmployees().FirstOrDefault(x => x.EmployeeId == 2),
                Message = responceMessage
            };

            mockEmployeeRepository.Setup(x => x.GetDataById(It.IsAny<Expression<Func<Employee, bool>>>()))
                                   .ReturnsAsync(response);

            // Act
            var result = await employeeServices.EditEmployeeAsync(id, employee);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal($"Employee with ID {id} not found", result.Message);
            mockEmployeeRepository.Verify(x => x.GetDataById(It.IsAny<Expression<Func<Employee, bool>>>()), Times.Once);
            mockEmployeeRepository.VerifyNoOtherCalls(); // Ensure no other methods on repository were called
        }

        [Fact]
        public async Task EditEmployeeAsync_SuccessfullyEditsData_ReturnsSuccessMessage()
        {
            // Arrange
            int id = 1;
            Employee employee = new Employee
            {
                EmployeeId = id,
                FullName = "John Doe",
            };
            var employeeResponce = new ResponceMessage<Employee>
            {
                IsSuccess = true,
                Data = GetTestEmployees().FirstOrDefault(x => x.EmployeeId == id)
            };
            var response = new ResponceMessage<Employee>
            {
                IsSuccess = true,
                Data = employee
            };

            mockEmployeeRepository.Setup(x => x.GetDataById(It.IsAny<Expression<Func<Employee, bool>>>()))
                                   .ReturnsAsync(employeeResponce);
            mockEmployeeRepository.Setup(x => x.EditData(It.IsAny<Employee>()))
                                   .ReturnsAsync(response);

            // Act
            var result = await employeeServices.EditEmployeeAsync(id, employee);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(employee.FullName, response.Data.FullName);
            mockEmployeeRepository.Verify(x => x.GetDataById(It.IsAny<Expression<Func<Employee, bool>>>()), Times.Once);
            mockEmployeeRepository.Verify(x => x.EditData(It.IsAny<Employee>()), Times.Once);
            mockEmployeeRepository.VerifyNoOtherCalls();
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
