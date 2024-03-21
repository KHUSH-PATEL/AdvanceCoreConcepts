using CoreAdvanceConcepts.Models;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MySqlX.XDevAPI.Common;
using System.Net;
using System.Net.Http;
using System.Text.Json;

namespace CoreAdvanceConcepts.nUnitTests.Controllers
{
    [TestFixture]
    public class Tests
    {
        private List<Employee> employeeList = new List<Employee>();
        private Mock<IEmployeeService> mockEmployeeService;
        private EmployeeController employeeController;

        [SetUp]
        public void Setup()
        {
            employeeList = GetTestEmployees();
            mockEmployeeService = new Mock<IEmployeeService>();
            employeeController = new EmployeeController(mockEmployeeService.Object);
        }

        [Test]
        public async Task GetEmployeesWhenOkResult()
        {
            // Arrange
            var response = new ResponceMessage<IEnumerable<Employee>>
            {
                IsSuccess = true,
                Data = employeeList
            };
            mockEmployeeService.Setup(service => service.GetEmployeesAsync())
                               .ReturnsAsync(response);

            // Act
            var result = await employeeController.GetEmployees();

            // Assert
            var okResult = result.Result as OkObjectResult;

            // Assert
            Assert.IsNotNull(okResult);
            Assert.IsInstanceOf<ResponceMessage<IEnumerable<Employee>>>(okResult.Value);
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            mockEmployeeService.Verify(service => service.GetEmployeesAsync(), Times.Once);
        }

        [Test]
        public async Task GetEmployeesWhenBadRequestResult()
        {
            // Arrange
            var response = new ResponceMessage<IEnumerable<Employee>>
            {
                IsSuccess = false,
                Data = employeeList
            };
            mockEmployeeService.Setup(service => service.GetEmployeesAsync())
                               .ReturnsAsync(response);

            // Act
            var actionResult = await employeeController.GetEmployees();
            var badRequestResult = actionResult.Result as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(badRequestResult);
            var responseMessage = badRequestResult.Value as ResponceMessage<IEnumerable<Employee>>;
            Assert.IsNotNull(responseMessage);
            Assert.IsFalse(responseMessage.IsSuccess);
            Assert.That(badRequestResult.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
            mockEmployeeService.Verify(service => service.GetEmployeesAsync(), Times.Once);
        }

        [Test]
        [TestCase(1)]
        [TestCase(5)]
        [TestCase(6)]
        public async Task GetEmployeeByIdWhenOkResult(int id)
        {
            // Arrange
            var response = new ResponceMessage<Employee>
            {
                IsSuccess = true,
                Data = employeeList.Find(x => x.EmployeeId == id)
            };
            mockEmployeeService.Setup(service => service.GetEmployeeByIdAsync(id))
                               .ReturnsAsync(response);

            // Act
            var controllerResult = await employeeController.GetEmployeeById(id);

            // Assert
            Assert.IsInstanceOf<ActionResult<Employee>>(controllerResult);
            var okResult = controllerResult.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var responseMessage = okResult.Value as ResponceMessage<Employee>;
            Assert.IsNotNull(responseMessage);
            Assert.IsTrue(responseMessage.IsSuccess);
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.IsNotNull(responseMessage.Data);
            Assert.That(responseMessage.Data, Is.EqualTo(response.Data));
            mockEmployeeService.Verify(service => service.GetEmployeeByIdAsync(id), Times.Once);
        }

        [Test]
        [TestCase(11)]
        [TestCase(15)]
        [TestCase(62)]
        public async Task GetEmployeeByIdWhenNotFoundResult(int id)
        {
            // Arrange
            var response = new ResponceMessage<Employee>
            {
                IsSuccess = false,
                Message = $"No employee found with Id: {id}",
                Data = null
            };
            mockEmployeeService.Setup(service => service.GetEmployeeByIdAsync(id))
                               .ReturnsAsync(response);

            // Act
            var controllerResult = await employeeController.GetEmployeeById(id);

            // Assert
            Assert.IsInstanceOf<ActionResult<Employee>>(controllerResult);
            var notFoundResult = controllerResult.Result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            var responseMessage = notFoundResult.Value as ResponceMessage<Employee>;
            Assert.IsNotNull(responseMessage);
            Assert.IsFalse(responseMessage.IsSuccess);
            Assert.That(notFoundResult.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
            Assert.IsNotNull(responseMessage);
            mockEmployeeService.Verify(service => service.GetEmployeeByIdAsync(id), Times.Once);
        }

        [Test]
        public async Task CreateEmployeeWhenCreatedAtActionResult()
        {
            // Arrange
            var employee = new Employee()
            {
                EmployeeId = 11,
                FullName = "Om Tank",
                City = "Hamilton",
                Email = "omii2013@gmail.com",
                PhoneNumber = "8578547858"
            };
            var response = new ResponceMessage<Employee>
            {
                IsSuccess = true,
                Data = employee
            };
            mockEmployeeService.Setup(service => service.CreateEmployeeAsync(employee))
                               .ReturnsAsync(response);

            // Act
            var controllerResult = await employeeController.CreateEmployee(employee);

            // Assert
            Assert.IsInstanceOf<ActionResult<Employee>>(controllerResult);
            var createdAtActionResult = controllerResult.Result as CreatedAtActionResult;
            Assert.IsNotNull(createdAtActionResult);
            var responseMessage = createdAtActionResult.Value as ResponceMessage<Employee>;
            Assert.IsNotNull(responseMessage);
            Assert.IsTrue(responseMessage.IsSuccess);
            Assert.That(createdAtActionResult.StatusCode, Is.EqualTo(StatusCodes.Status201Created));
            Assert.IsNotNull(responseMessage.Data);
            Assert.That(responseMessage.Data, Is.EqualTo(employee));
            mockEmployeeService.Verify(service => service.CreateEmployeeAsync(employee), Times.Once);
        }

        [Test]
        [TestCase(1)]
        [TestCase(5)]
        [TestCase(6)]
        public async Task EditEmployeeWhenOkResult(int id)
        {
            var employee = employeeList.Find(x => x.EmployeeId == id);
            // Arrange
            var response = new ResponceMessage<Employee>
            {
                IsSuccess = true,
                Data = employee
            };
            mockEmployeeService.Setup(setup => setup.EditEmployeeAsync(employee.EmployeeId, employee))
                              .ReturnsAsync(response);

            // Act
            var controllerResult = await employeeController.EditEmployee(employee.EmployeeId, employee);

            // Assert
            Assert.IsNotNull(controllerResult);
            Assert.IsInstanceOf<ActionResult<Employee>>(controllerResult);
            var okResult = controllerResult.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var responseMessage = okResult.Value as ResponceMessage<Employee>;
            Assert.IsNotNull(responseMessage);
            Assert.IsTrue(responseMessage.IsSuccess);
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(okResult.Value, Is.EqualTo(response));
        }

        [Test]
        [TestCase(1)]
        [TestCase(5)]
        [TestCase(6)]
        public async Task EditEmployeeWhenBadRequestResult(int id)
        {
            // Arrange
            var employee = employeeList.Find(x => x.EmployeeId == id);
            var response = new ResponceMessage<Employee>
            {
                IsSuccess = false,
                Data = employee
            };
            mockEmployeeService.Setup(setup => setup.EditEmployeeAsync(employee.EmployeeId, employee))
                              .ReturnsAsync(response);

            // Act
            var controllerResult = await employeeController.EditEmployee(employee.EmployeeId, employee);

            // Assert
            Assert.IsNotNull(controllerResult);
            Assert.IsInstanceOf<ActionResult<Employee>>(controllerResult);
            var badRequestResult = controllerResult.Result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            var responseMessage = badRequestResult.Value as ResponceMessage<Employee>;
            Assert.IsNotNull(responseMessage);
            Assert.IsFalse(responseMessage.IsSuccess);
            Assert.That(badRequestResult.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
            Assert.That(badRequestResult.Value, Is.EqualTo(response));
        }

        [TearDown]
        public void TearDown()
        {
            mockEmployeeService.Reset();
        }

        public List<Employee> GetTestEmployees()
        {
            return new List<Employee>
            {
                new Employee { EmployeeId = 1, FullName = "Khush Makadiya", City = "Rajkot", PhoneNumber = "9898989898", Email = "mk3@gmail.com", FlagDeleted = false },
                new Employee { EmployeeId = 2, FullName = "Jenish Raiyani", City = "Junagadh", PhoneNumber = "8527418528", Email = "JR3@gmail.com", FlagDeleted = false },
                new Employee { EmployeeId = 3, FullName = "Akash Rana", City = "Chonga", PhoneNumber = "8989895623", Email = "AR3@gmail.com", FlagDeleted = false },
                new Employee { EmployeeId = 4, FullName = "Some Other Name", City = "Some City", PhoneNumber = "1234567890", Email = "example@gmail.com", FlagDeleted = false },
                new Employee { EmployeeId = 5, FullName = "Another Name", City = "Another City", PhoneNumber = "9876543210", Email = "another@example.com", FlagDeleted = false },
                new Employee { EmployeeId = 6, FullName = "Test Person", City = "Test City", PhoneNumber = "1112223333", Email = "test@example.com", FlagDeleted = false },
                new Employee { EmployeeId = 7, FullName = "John Doe", City = "New York", PhoneNumber = "5555555555", Email = "john@example.com", FlagDeleted = false },
                new Employee { EmployeeId = 8, FullName = "Jane Smith", City = "Los Angeles", PhoneNumber = "4444444444", Email = "jane@example.com", FlagDeleted = false},
                new Employee { EmployeeId = 9, FullName = "Alice Johnson", City = "Chicago", PhoneNumber = "3333333333", Email = "alice@example.com", FlagDeleted = false },
                new Employee { EmployeeId = 10, FullName = "Bob Brown", City = "Houston", PhoneNumber = "2222222222", Email = "bob@example.com", FlagDeleted = false }
            };
        }
    }
}