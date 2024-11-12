// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Extensions.Options;
// using Moq;
// using TokenService.Controllers;
// using TokenService.Models;
// using TokenService.Models.FormModels;
// using TokenService.Models.ResponseModels;
// using TokenService.Services;
//
// namespace TokenService.Tests;
//
// public class SimpleTests
// {
//     private Mock<IOptions<ApiSettings>> _mockApiSettings;
//     // private readonly Mock<TokenGeneratorService> _mockTokenGeneratorService;
//     private readonly TokenGeneratorController _controller;
//
//     public SimpleTests()
//     {
//         _mockApiSettings = new Mock<IOptions<ApiSettings>>();
//     }
//
//     [Fact]
//     public void Test1()
//     {
//         _mockApiSettings = new Mock<IOptions<ApiSettings>>();
//         // _mockTokenGeneratorService = new Mock<TokenGeneratorService>();
//
//
//         _mockApiSettings.Setup(a => a.Value).Returns(new ApiSettings
//         {
//             BaseUrl = "https://identity.example.com"
//         });
//
//         _controller = new TokenGeneratorController(
//             _mockApiSettings.Object
//         );
//     }
//
//     [Fact]
//     public async Task Login_AsAdmin_ReturnsTokenWithAdminRole()
//     {
//         // Arrange: Mock admin login response and token generation
//         var loginRequest = new LoginModel { Email = "adminuser", Password = "adminpassword" };
//
//         var adminResponseContent = new ResponseContent
//         {
//             Id = "admin-id",
//             Email = "admin@example.com",
//             Roles = ["Admin"]
//         };
//
//         var responseResult = new ResponseResult { ResponseContent = adminResponseContent };
//
//         // Mock TokenGeneratorService to return a JWT token for the admin
//         // _mockTokenGeneratorService
//         //     .Setup(s => s.GenerateAccessToken(adminResponseContent))
//         //     .Returns("mocked-admin-jwt-token");
//
//         // Act: Call the Login method
//         // var result = await _controller.Login(loginRequest) as OkObjectResult;
//
//         // Assert: Check that the response is Ok and the token contains admin role
//         Assert.NotNull(result);
//         Assert.Equal(200, result.StatusCode);
//
//         // Optionally, verify the JWT token has been generated with "Admin" role
//         var responseValue = result.Value as dynamic;
//         Assert.Equal("Success!", responseValue?.Message);
//
//         // Additional check to ensure that the service's GenerateAccessToken was called with the correct content
//         _mockTokenGeneratorService.Verify(
//             s => s.GenerateAccessToken(It.Is<ResponseContent>(c => c.Roles.Contains("Admin"))),
//             Times.Once
//         );
//     }
// }