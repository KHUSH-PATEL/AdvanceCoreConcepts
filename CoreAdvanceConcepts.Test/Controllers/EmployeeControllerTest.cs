using CoreAdvanceConcepts.Models;
using Microsoft.AspNetCore.Http;
using Moq;
using MySqlX.XDevAPI.Common;

namespace CoreAdvanceConcepts.Test.Controllers
{
    public class EmployeeControllerTest
    {
        private readonly Mock<IEmployeeService> mockEmployeeService;
        private readonly EmployeeController employeeController;
        public EmployeeControllerTest()
        {
            //Arrange
            mockEmployeeService = new Mock<IEmployeeService>();
            employeeController = new EmployeeController(mockEmployeeService.Object);

        }

        [Fact]
        public async Task GetEmployees_Employees_OkResult()
        {
            //Arrange
            var employees = GetTestEmployees();

            var response = new ResponceMessage<IEnumerable<Employee>>
            {
                IsSuccess = true,
                Data = employees
            };
            mockEmployeeService.Setup(service => service.GetEmployeesAsync())
                               .ReturnsAsync(response);

            // Act
            var result = await employeeController.GetEmployees();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Employee>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.IsAssignableFrom<ResponceMessage<IEnumerable<Employee>>>(okResult.Value);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            mockEmployeeService.Verify(service => service.GetEmployeesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetEmployees_Employees_BadRequestResult()
        {
            //Arrange
            var employees = GetTestEmployees();

            var response = new ResponceMessage<IEnumerable<Employee>>
            {
                IsSuccess = false,
                Data = employees
            };
            mockEmployeeService.Setup(service => service.GetEmployeesAsync())
                               .ReturnsAsync(response);

            // Act
            var result = await employeeController.GetEmployees();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Employee>>>(result);
            var okResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            var responseMessage = Assert.IsAssignableFrom<ResponceMessage<IEnumerable<Employee>>>(okResult.Value);
            Assert.False(responseMessage.IsSuccess);
            Assert.Equal(StatusCodes.Status400BadRequest, okResult.StatusCode);
            mockEmployeeService.Verify(service => service.GetEmployeesAsync(), Times.Once);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(6)]
        public async Task GetEmployeeById_Employee_OkResult(int id)
        {
            //Arrange
            var employee = GetTestEmployees().Find(x => x.EmployeeId == id);
            var response = new ResponceMessage<Employee>
            {
                IsSuccess = true,
                Data = employee
            };
            mockEmployeeService.Setup(service => service.GetEmployeeByIdAsync(id))
                               .ReturnsAsync(response);

            //Act
            var controllerResult = await employeeController.GetEmployeeById(id);

            //Assert
            var actionResult = Assert.IsType<ActionResult<Employee>>(controllerResult);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var responseMessage = Assert.IsAssignableFrom<ResponceMessage<Employee>>(okResult.Value);
            Assert.True(responseMessage.IsSuccess);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.NotNull(responseMessage.Data);
            Assert.Equal(employee, responseMessage.Data);
            mockEmployeeService.Verify(service => service.GetEmployeeByIdAsync(id), Times.Once);
        }

        [Theory]
        [InlineData(11)]
        [InlineData(15)]
        [InlineData(62)]
        public async Task GetEmployeeById_Employee_NotFoundResult(int id)
        {
            //Arrange
            var employee = GetTestEmployees().Find(x => x.EmployeeId == id);
            var response = new ResponceMessage<Employee>
            {
                IsSuccess = false,
                Message = $"No employee found with Id: {id}",
                Data = employee
            };
            mockEmployeeService.Setup(service => service.GetEmployeeByIdAsync(id))
                               .ReturnsAsync(response);

            //Act
            var controllerResult = await employeeController.GetEmployeeById(id);

            //Assert
            var actionResult = Assert.IsType<ActionResult<Employee>>(controllerResult);
            var okResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
            var responseMessage = Assert.IsAssignableFrom<ResponceMessage<Employee>>(okResult.Value);
            Assert.False(responseMessage.IsSuccess);
            Assert.Equal(StatusCodes.Status404NotFound, okResult.StatusCode);
            Assert.NotNull(responseMessage);
            Assert.Equal(employee, responseMessage.Data);
            mockEmployeeService.Verify(service => service.GetEmployeeByIdAsync(id), Times.Once);
        }

        [Fact]
        public async Task CreateEmployee_Employee_CreatedAtActionResult()
        {
            //Arrange
            Employee employee = new Employee()
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

            //Act
            var controllerResult = await employeeController.CreateEmployee(employee);

            //Assert
            var actionResult = Assert.IsType<ActionResult<Employee>>(controllerResult);
            var okResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var responseMessage = Assert.IsAssignableFrom<ResponceMessage<Employee>>(okResult.Value);
            Assert.True(responseMessage.IsSuccess);
            Assert.Equal(StatusCodes.Status201Created, okResult.StatusCode);
            Assert.NotNull(responseMessage.Data);
            Assert.Equal(employee, responseMessage.Data);
            mockEmployeeService.Verify(service => service.CreateEmployeeAsync(employee), Times.Once);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(6)]
        public async Task EditEmployee_Employee_OkResult(int id)
        {
            // Arrange
            var employee = GetTestEmployees().Find(x => x.EmployeeId == id);
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
            Assert.NotNull(controllerResult);
            var actionResult = Assert.IsType<ActionResult<Employee>>(controllerResult);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var responseMessage = Assert.IsAssignableFrom<ResponceMessage<Employee>>(okResult.Value);
            Assert.True(responseMessage.IsSuccess);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.Equal(response, okResult.Value);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(6)]
        public async Task EditEmployee_Employee_BadRequestResult(int id)
        {
            // Arrange
            var employee = GetTestEmployees().Find(x => x.EmployeeId == id);
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
            var actionResult = Assert.IsType<ActionResult<Employee>>(controllerResult);
            var okResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            var responseMessage = Assert.IsAssignableFrom<ResponceMessage<Employee>>(okResult.Value);
            Assert.False(responseMessage.IsSuccess);
            Assert.NotNull(controllerResult);
            Assert.Equal(StatusCodes.Status400BadRequest, okResult.StatusCode);
            Assert.Equal(response, okResult.Value);
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