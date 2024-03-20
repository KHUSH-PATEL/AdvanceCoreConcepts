using CoreAdvanceConcepts.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CoreAdvanceConcepts.nUnitTests.Services
{
    [TestFixture]
    internal class EmployeeServiceTest
    {
        private List<Employee> employeeList = new List<Employee>();
        private Mock<IRepository<Employee>> mockEmployeeRepository;
        private EmployeeServices employeeServices;

        [SetUp]
        public void Setup()
        {
            employeeList = GetTestEmployees().Where(x => !x.FlagDeleted).ToList();
            mockEmployeeRepository = new Mock<IRepository<Employee>>();
            employeeServices = new EmployeeServices(mockEmployeeRepository.Object);
        }

        [Test]
        public async Task GetEmployeesAsyncWhenReturnsOkResult()
        {
            // Arrange
            var response = new ResponceMessage<IEnumerable<Employee>>
            {
                IsSuccess = true,
                Data = employeeList
            };
            mockEmployeeRepository.Setup(setup => setup.GetDataList())
                                  .ReturnsAsync(response);
            var employee = response.Data.Where(x => !x.FlagDeleted).ToList();

            // Act
            var serviceResult = await employeeServices.GetEmployeesAsync();

            // Assert
            Assert.IsTrue(serviceResult.IsSuccess);
            Assert.IsNotNull(serviceResult.Data);
            Assert.That(serviceResult.DataCount, Is.EqualTo(employeeList.Count));
            Assert.That(serviceResult.Data, Is.EqualTo(employee));
            mockEmployeeRepository.Verify(x => x.GetDataList(), Times.Once);
        }

        [Test]
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
            Assert.That(serviceResult.IsSuccess, Is.True);
            Assert.That(serviceResult.Data, Is.Null);
            Assert.That(serviceResult.Message, Is.EqualTo(message));
            mockEmployeeRepository.Verify(x => x.GetDataList(), Times.Once);
        }

        [Test]
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
            Assert.That(serviceResult.Message, Is.EqualTo(responseMessage));
            Assert.IsNotNull(serviceResult.ErrorMessage);
            Assert.That(serviceResult.ErrorMessage.Count, Is.EqualTo(1));
            Assert.That(serviceResult.ErrorMessage[0], Is.EqualTo(errorMessage));
            mockEmployeeRepository.Verify(x => x.GetDataList(), Times.Once);
        }

        [TestCase(1)]
        [TestCase(5)]
        [TestCase(7)]
        public async Task GetEmployeeByIdAsyncWhenReturnsOkResult(int id)
        {
            var employees = employeeList.FirstOrDefault(x => x.EmployeeId == id);
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
            Assert.IsTrue(serviceResult.IsSuccess);
            Assert.IsNotNull(serviceResult.Data);
            Assert.IsFalse(serviceResult.Data.FlagDeleted);
            mockEmployeeRepository.Verify(x => x.GetDataById(It.IsAny<Expression<Func<Employee, bool>>>()), Times.Once);
        }

        [TestCase(1)]
        [TestCase(5)]
        [TestCase(7)]
        public async Task GetEmployeeByIdAsyncWhenReturnsFailureResult(int id)
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
            Assert.That(result.Message, Is.EqualTo(responseMessage));
            Assert.IsNotNull(result.ErrorMessage);
            Assert.That(result.ErrorMessage[0], Is.EqualTo(errorMessage));
        }

        [TestCase(1)]
        [TestCase(5)]
        [TestCase(7)]
        public async Task DeleteEmployeeAsyncWhenEmployeeExists(int id)
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
            Assert.IsNotNull(serviceResult.Data);
            Assert.IsTrue(response.IsSuccess);
            Assert.That(response.Message, Is.EqualTo(string.Empty));
            Assert.IsTrue(serviceResult.Data.FlagDeleted);
            Assert.IsTrue(serviceResult.Data.UpdatedDate > DateTime.MinValue);
            mockEmployeeRepository.Verify(x => x.DeleteData(serviceResult.Data), Times.Once);
        }

        [TestCase(2)]
        [TestCase(4)]
        [TestCase(6)]
        public async Task DeleteEmployeeAsyncWhenEmployeeNotExists(int id)
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
            Assert.That(response.Message, Is.EqualTo(responseMessage));
            mockEmployeeRepository.Verify(x => x.GetDataById(It.IsAny<Expression<Func<Employee, bool>>>()), Times.Once);
        }

        [TestCase(1)]
        [TestCase(3)]
        [TestCase(10)]
        public async Task EditEmployeeAsyncWhenIdDoesNotMatchEmployeeIdReturnsErrorMessage(int id)
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
            Assert.That(result.Message, Is.EqualTo(responseMessage));
            mockEmployeeRepository.Verify(x => x.GetDataById(It.IsAny<Expression<Func<Employee, bool>>>()), Times.Once);
        }

        [Test]
        public async Task EditEmployeeAsyncWhenFlagDeletedIsTrueReturnsErrorMessage()
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
            Assert.That(result.Message, Is.EqualTo(responseMessage));
            mockEmployeeRepository.Verify(x => x.GetDataById(It.IsAny<Expression<Func<Employee, bool>>>()), Times.Once);
            mockEmployeeRepository.VerifyNoOtherCalls();
        }

        [Test]
        public async Task EditEmployeeAsyncWhenSuccessfullyEditsDataReturnsSuccessMessage()
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
            Assert.That(response.Data.FullName, Is.EqualTo(employee.FullName));
            mockEmployeeRepository.Verify(x => x.GetDataById(It.IsAny<Expression<Func<Employee, bool>>>()), Times.Once);
            mockEmployeeRepository.Verify(x => x.EditData(It.IsAny<Employee>()), Times.Once);
            mockEmployeeRepository.VerifyNoOtherCalls();
        }


        [TearDown]
        public void TearDown()
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
                new Employee { EmployeeId = 8, FullName = "Jane Smith", City = "Los Angeles", PhoneNumber = "4444444444", Email = "jane@example.com", FlagDeleted = false},
                new Employee { EmployeeId = 9, FullName = "Alice Johnson", City = "Chicago", PhoneNumber = "3333333333", Email = "alice@example.com", FlagDeleted = false },
                new Employee { EmployeeId = 10, FullName = "Bob Brown", City = "Houston", PhoneNumber = "2222222222", Email = "bob@example.com", FlagDeleted = false }
            };
        }
    }
}
