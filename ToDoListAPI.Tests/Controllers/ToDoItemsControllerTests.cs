using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using ToDoListAPI.Controllers;
using ToDoListAPI.DTOs;
using ToDoListAPI.Models;
using ToDoListAPI.Services;
using Xunit;

public class ToDoItemsControllerTests
{
    private readonly Mock<IToDoItemService> _mockService;
    private readonly Mock<ILogger<ToDoItemsController>> _mockLogger;
    private readonly ToDoItemsController _controller;

    public ToDoItemsControllerTests()
    {
        _mockService = new Mock<IToDoItemService>();
        _mockLogger = new Mock<ILogger<ToDoItemsController>>();
        _controller = new ToDoItemsController(_mockService.Object, _mockLogger.Object);
    }

    private void SetupUser(int userId)
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }));
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };
    }

    [Fact]
    public async Task GetToDoItems_ValidRequest_ReturnsOkResult()
    {
        // Arrange
        SetupUser(1);
        var page = 1;
        var limit = 10;
        var items = new PagedResult<ToDoItem> { Data = new List<ToDoItem>(), Total = 0 };
        _mockService.Setup(s => s.GetItemsByUserIdAsync(1, page, limit, null, null))
            .ReturnsAsync(items);

        // Act
        var result = await _controller.GetToDoItems(page, limit);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(items, okResult.Value);
    }

    [Fact]
    public async Task GetToDoItems_InvalidPage_ReturnsBadRequest()
    {
        // Arrange
        var page = 0; // Invalid page
        var limit = 10;

        // Act
        var result = await _controller.GetToDoItems(page, limit);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("The 'page' parameter must be greater than 0.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetToDoItems_ServiceThrowsException_ReturnsServerError()
    {
        // Arrange
        SetupUser(1);
        var page = 1;
        var limit = 10;
        _mockService.Setup(s => s.GetItemsByUserIdAsync(1, page, limit, null, null))
            .ThrowsAsync(new Exception("Service error")); // Simula un error en el servicio

        // Act
        var result = await _controller.GetToDoItems(page, limit);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        Assert.Equal("An error occurred while retrieving the to-do items.", objectResult.Value);
    }


    [Fact]
    public async Task GetToDoItemById_ItemFound_ReturnsOkResult()
    {
        // Arrange
        SetupUser(1);
        var itemId = 1;
        var item = new ToDoItem { ToDoItemId = itemId };
        _mockService.Setup(s => s.GetItemByIdAsync(itemId)).ReturnsAsync(item);

        // Act
        var result = await _controller.GetToDoItemById(itemId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(item, okResult.Value);
    }

    [Fact]
    public async Task GetToDoItemById_ItemNotFound_ReturnsNotFound()
    {
        // Arrange
        SetupUser(1);
        var itemId = 1;
        _mockService.Setup(s => s.GetItemByIdAsync(itemId)).ReturnsAsync((ToDoItem)null);

        // Act
        var result = await _controller.GetToDoItemById(itemId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task CreateToDoItem_ValidRequest_ReturnsCreatedResult()
    {
        // Arrange
        SetupUser(1);
        var newItem = new ToDoItemRequest { Title = "New Item", Description = "Description" };
        var createdItem = new ToDoItem { ToDoItemId = 1, Title = newItem.Title, Description = newItem.Description };
        _mockService.Setup(s => s.AddItemAsync(1, newItem)).ReturnsAsync(createdItem);

        // Act
        var result = await _controller.CreateToDoItem(newItem);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(_controller.GetToDoItemById), createdAtActionResult.ActionName);
        Assert.Equal(createdItem.ToDoItemId, ((ToDoItem)createdAtActionResult.Value).ToDoItemId);
    }

    [Fact]
    public async Task CreateToDoItem_InvalidModel_ReturnsBadRequest()
    {
        // Arrange
        _controller.ModelState.AddModelError("Title", "Required");

        // Act
        var result = await _controller.CreateToDoItem(new ToDoItemRequest());

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var errors = Assert.IsType<SerializableError>(badRequestResult.Value);
        Assert.True(errors.ContainsKey("Title"));
        Assert.Contains("Required", (IEnumerable<string>)errors["Title"]);
    }


    [Fact]
    public async Task UpdateToDoItem_ItemFound_ReturnsOkResult()
    {
        // Arrange
        SetupUser(1);
        var itemId = 1;
        var itemRequest = new ToDoItemRequest { Title = "Updated Item" };
        var updatedItem = new ToDoItem { ToDoItemId = itemId, Title = itemRequest.Title };
        _mockService.Setup(s => s.UpdateItemAsync(itemId, itemRequest)).ReturnsAsync(updatedItem);

        // Act
        var result = await _controller.UpdateToDoItem(itemId, itemRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(updatedItem, okResult.Value);
    }

    [Fact]
    public async Task UpdateToDoItem_ItemNotFound_ReturnsNotFound()
    {
        // Arrange
        SetupUser(1);
        var itemId = 1;
        var itemRequest = new ToDoItemRequest { Title = "Updated Item" };
        _mockService.Setup(s => s.UpdateItemAsync(itemId, itemRequest)).ReturnsAsync((ToDoItem)null);

        // Act
        var result = await _controller.UpdateToDoItem(itemId, itemRequest);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteToDoItem_ItemFound_ReturnsNoContent()
    {
        // Arrange
        SetupUser(1);
        var itemId = 1;
        _mockService.Setup(s => s.DeleteItemAsync(itemId)).ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteToDoItem(itemId);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteToDoItem_ItemNotFound_ReturnsNotFound()
    {
        // Arrange
        var itemId = 1;
        SetupUser(1);
        _mockService.Setup(s => s.DeleteItemAsync(itemId))
            .ReturnsAsync(false); // Simula que el ítem no fue encontrado

        // Act
        var result = await _controller.DeleteToDoItem(itemId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task DeleteToDoItem_ServiceThrowsException_ReturnsServerError()
    {
        // Arrange
        var itemId = 1;
        SetupUser(1);
        _mockService.Setup(s => s.DeleteItemAsync(itemId))
            .ThrowsAsync(new Exception("Service error")); // Simula un error en el servicio

        // Act
        var result = await _controller.DeleteToDoItem(itemId);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        Assert.Equal("An error occurred while deleting the to-do item.", objectResult.Value);
    }

}
