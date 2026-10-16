using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using RaceDay.API.Controllers;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using Xunit;

namespace RaceDay.Tests
{
    public class AuthControllerTests
    {
        private readonly RaceDayDbContext _context;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new RaceDayDbContext(options);

            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c["Jwt:Key"]).Returns("ThisIsMySuperSecretKeyForRaceDayAPI12345!");
            mockConfig.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
            mockConfig.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");

            // 3. Create the controller with the test context and mocked config
            _controller = new AuthController(_context, mockConfig.Object);
        }

        [Fact]
        public async Task Register_ValidOrganiser_ReturnsOkResult()
        {
            // Arrange
            var request = new RegisterDto
            {
                Email = "testorganiser@test.com",
                Password = "Password123!",
                FirstName = "Test",
                LastName = "Organiser",
                Phone = "0821234567",
                Role = "Organiser",
                Organisation = "Test Org"
            };

            // Act
            var result = await _controller.Register(request);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            Assert.Single(_context.Organisers); // Check that the organiser was actually saved to the DB
        }

        [Fact]
        public async Task Register_DuplicateEmail_ReturnsBadRequest()
        {
            // Arrange
            var request = new RegisterDto
            {
                Email = "duplicate@test.com",
                Password = "Password123!",
                FirstName = "Test",
                LastName = "User",
                Phone = "0821234567",
                Role = "Participant",
                DateOfBirth = new DateTime(1990, 1, 1),
                Gender = "Male"
            };
            await _controller.Register(request); // Register the first time

            //Act
            var result = await _controller.Register(request);

            //Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkWithToken()
        {
            //Arrange
            var registerRequest = new RegisterDto
            {
                Email = "login@test.com",
                Password = "Password123!",
                FirstName = "Test",
                LastName = "User",
                Phone = "0821234567",
                Role = "Participant",
                DateOfBirth = new DateTime(1990, 1, 1),
                Gender = "Male"
            };
            await _controller.Register(registerRequest);

            var loginRequest = new LoginDto
            {
                Email = "login@test.com",
                Password = "Password123!"
            };

            //Act
            var result = await _controller.Login(loginRequest);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<AuthResponseDto>(okResult.Value);
            Assert.NotNull(response.Token);
            Assert.Equal("Participant", response.Role);
        }

        [Fact]
        public async Task Login_InvalidPassword_ReturnsBadRequest()
        {
            //Arrange
            var registerRequest = new RegisterDto
            {
                Email = "wrongpass@test.com",
                Password = "Password123!",
                FirstName = "Test",
                LastName = "User",
                Phone = "0821234567",
                Role = "Participant",
                DateOfBirth = new DateTime(1990, 1, 1),
                Gender = "Male"
            };
            await _controller.Register(registerRequest);

            var loginRequest = new LoginDto
            {
                Email = "wrongpass@test.com",
                Password = "WrongPassword!"
            };

            //Act
            var result = await _controller.Login(loginRequest);

            //Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}