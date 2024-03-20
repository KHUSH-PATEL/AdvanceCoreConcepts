using CoreAdvanceConcepts.Controllers;
using CoreAdvanceConcepts.Interface;
using CoreAdvanceConcepts.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CoreAdvanceConcepts.MsUnitTest.Controllers
{
    [TestClass]
    public class EmployeeControllerTest
    {
        private Mock<IEmployeeService> mockEmployeeService;
        private EmployeeController employeeController;
        private List<Employee> employeeList = new List<Employee>();

        [TestInitialize]
        public void Initialize()
        {
            mockEmployeeService = new Mock<IEmployeeService>();
            employeeController = new EmployeeController(mockEmployeeService.Object);
            employeeList = GetTestEmployees();
        }

        [TestMethod]
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
            Assert.IsInstanceOfType(okResult.Value, typeof(ResponceMessage<IEnumerable<Employee>>));
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
            mockEmployeeService.Verify(service => service.GetEmployeesAsync(), Times.Once);
        }
        [TestMethod]
        public async Task GetEmployees_WhenBadRequestResult()
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
            Assert.AreEqual(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
            mockEmployeeService.Verify(service => service.GetEmployeesAsync(), Times.Once);
        }

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(5)]
        [DataRow(6)]
        public async Task GetEmployeeById_WhenOkResult(int id)
        {
            // Arrange
            var response = new ResponceMessage<Employee>
            {
                IsSuccess = true,
                Data = employeeList.FirstOrDefault(x => x.EmployeeId == id)
            };
            mockEmployeeService.Setup(service => service.GetEmployeeByIdAsync(id))
                               .ReturnsAsync(response);

            // Act
            var controllerResult = await employeeController.GetEmployeeById(id);

            // Assert
            Assert.IsInstanceOfType(controllerResult, typeof(ActionResult<Employee>));
            var okResult = controllerResult.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var responseMessage = okResult.Value as ResponceMessage<Employee>;
            Assert.IsNotNull(responseMessage);
            Assert.IsTrue(responseMessage.IsSuccess);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.IsNotNull(responseMessage.Data);
            Assert.AreEqual(response.Data, responseMessage.Data);
            mockEmployeeService.Verify(service => service.GetEmployeeByIdAsync(id), Times.Once);
        }

        [DataTestMethod]
        [DataRow(11)]
        [DataRow(15)]
        [DataRow(62)]
        public async Task GetEmployeeById_WhenNotFoundResult(int id)
        {
            // Arrange
            string responceMessage = $"No employee found with Id: {id}";
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
            Assert.IsInstanceOfType(controllerResult, typeof(ActionResult<Employee>));
            var notFoundResult = controllerResult.Result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            var responseMessage = notFoundResult.Value as ResponceMessage<Employee>;
            Assert.IsNotNull(responseMessage);
            Assert.IsFalse(responseMessage.IsSuccess);
            Assert.AreEqual(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
            Assert.IsNotNull(responseMessage.Message);
            Assert.AreEqual(responceMessage, responseMessage.Message);
            mockEmployeeService.Verify(service => service.GetEmployeeByIdAsync(id), Times.Once);
        }

        [TestMethod]
        public async Task CreateEmployee_WhenCreatedAtActionResult()
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
            Assert.IsInstanceOfType(controllerResult, typeof(ActionResult<Employee>));
            var createdAtActionResult = controllerResult.Result as CreatedAtActionResult;
            Assert.IsNotNull(createdAtActionResult);
            var responseMessage = createdAtActionResult.Value as ResponceMessage<Employee>;
            Assert.IsNotNull(responseMessage);
            Assert.IsTrue(responseMessage.IsSuccess);
            Assert.AreEqual(StatusCodes.Status201Created, createdAtActionResult.StatusCode);
            Assert.IsNotNull(responseMessage.Data);
            Assert.AreEqual(employee, responseMessage.Data);
            mockEmployeeService.Verify(service => service.CreateEmployeeAsync(employee), Times.Once);
        }

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(5)]
        [DataRow(6)]
        public async Task EditEmployee_WhenOkResult(int id)
        {
            var employee = employeeList.FirstOrDefault(x => x.EmployeeId == id);
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
            Assert.IsInstanceOfType(controllerResult, typeof(ActionResult<Employee>));
            var okResult = controllerResult.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var responseMessage = okResult.Value as ResponceMessage<Employee>;
            Assert.IsNotNull(responseMessage);
            Assert.IsTrue(responseMessage.IsSuccess);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.AreEqual(response, okResult.Value);
        }

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(5)]
        [DataRow(6)]
        public async Task EditEmployee_WhenBadRequestResult(int id)
        {
            var employee = employeeList.FirstOrDefault(x => x.EmployeeId == id);
            // Arrange
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
            Assert.IsInstanceOfType(controllerResult, typeof(ActionResult<Employee>));
            var badRequestResult = controllerResult.Result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            var responseMessage = badRequestResult.Value as ResponceMessage<Employee>;
            Assert.IsNotNull(responseMessage);
            Assert.IsFalse(responseMessage.IsSuccess);
            Assert.AreEqual(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
            Assert.AreEqual(response, badRequestResult.Value);
        }

        [TestCleanup]
        public void Cleanup()
        {
            mockEmployeeService.Reset();
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