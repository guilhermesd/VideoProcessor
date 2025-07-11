using Application.UseCases;
using Domain.Entities;
using Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;
using Moq;
using VideoProcessor.API.Controllers;

namespace Tests.Unidade
{
    public class AuthControllerTests
    {
        [Fact]
        public async Task Login_ComCredenciaisValidas_DeveRetornarOkComToken()
        {
            // Arrange
            var email = "user@email.com";
            var password = "senha123";
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            var user = new User { Id = Guid.NewGuid(), Email = email, PasswordHash = passwordHash };

            var userRepoMock = new Mock<IUserRepository>();
            userRepoMock.Setup(repo => repo.GetByEmailAsync(email)).ReturnsAsync(user);

            var authUseCaseMock = new Mock<IAuthUseCase>();
            authUseCaseMock.Setup(a => a.GenerateToken(user)).Returns("fake-jwt-token");

            var controller = new AuthController(authUseCaseMock.Object, userRepoMock.Object);
            var request = new LoginRequest { Email = email, Password = password };

            // Act
            var result = await controller.Login(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Contains("token", okResult.Value?.ToString());
        }

        [Fact]
        public async Task Login_ComCredenciaisInvalidas_DeveRetornarUnauthorized()
        {
            // Arrange
            var request = new LoginRequest { Email = "invalido@email.com", Password = "senha" };

            var userRepoMock = new Mock<IUserRepository>();
            userRepoMock.Setup(repo => repo.GetByEmailAsync(request.Email)).ReturnsAsync((User?)null);

            var authUseCaseMock = new Mock<IAuthUseCase>();

            var controller = new AuthController(authUseCaseMock.Object, userRepoMock.Object);

            // Act
            var result = await controller.Login(request);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Register_ComEmailNovo_DeveRetornarOk()
        {
            // Arrange
            var request = new RegisterRequest { Email = "novo@email.com", Password = "123" };

            var userRepoMock = new Mock<IUserRepository>();
            userRepoMock.Setup(repo => repo.GetByEmailAsync(request.Email)).ReturnsAsync((User?)null);
            userRepoMock.Setup(repo => repo.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

            var authUseCaseMock = new Mock<IAuthUseCase>();

            var controller = new AuthController(authUseCaseMock.Object, userRepoMock.Object);

            // Act
            var result = await controller.Register(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task Register_ComEmailExistente_DeveRetornarBadRequest()
        {
            // Arrange
            var request = new RegisterRequest { Email = "existente@email.com", Password = "123" };

            var existingUser = new User { Id = Guid.NewGuid(), Email = request.Email, PasswordHash = "..." };

            var userRepoMock = new Mock<IUserRepository>();
            userRepoMock.Setup(repo => repo.GetByEmailAsync(request.Email)).ReturnsAsync(existingUser);

            var authUseCaseMock = new Mock<IAuthUseCase>();

            var controller = new AuthController(authUseCaseMock.Object, userRepoMock.Object);

            // Act
            var result = await controller.Register(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
