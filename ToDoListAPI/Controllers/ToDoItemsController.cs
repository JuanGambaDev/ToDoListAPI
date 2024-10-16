using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using ToDoListAPI.DTOs;
using ToDoListAPI.Models;
using ToDoListAPI.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ToDoListAPI.Controllers
{
    /// <summary>
    /// Controller to handle task list operations.
    /// Provides endpoints to create, read, update, and delete task list items.
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ToDoItemsController : ControllerBase
    {
        private readonly IToDoItemService _toDoItemService;
        private readonly ILogger<ToDoItemsController> _logger;

        /// <summary>
        /// Controller constructor that injects the task list service and logger.
        /// </summary>
        /// <param name="toDoItemService">Service to manage tasks.</param>
        /// <param name="logger">Logger to record events and errors.</param>
        public ToDoItemsController(IToDoItemService toDoItemService, ILogger<ToDoItemsController> logger)
        {
            _toDoItemService = toDoItemService;
            _logger = logger;
        }

        /// <summary>
        /// Gets a paginated list of task list items.
        /// </summary>
        /// <param name="page">Page number for pagination (by default it is 1, it cannot be less than or equal to 0).</param>
        /// <param name="limit">Maximum number of elements per page (default is 10, cannot be less than or equal to 0).</param>
        /// <param name="filter">Optional filter that gets the tasks (ToDoItems) that contain in its id, title or description the value provided</param>
        /// <param name="sortBy">Optional field to sort the task list (valid parameters are id, title, description).</param>
        /// <returns>A response with to-do list items.</returns>
        [HttpGet]
        public async Task<IActionResult> GetToDoItems(int page = 1, int limit = 10, string filter = null, string sortBy = null)
        {
            if (page <= 0)
            {
                _logger.LogWarning("Invalid page parameter: {Page}. It must be greater than 0.", page);
                return BadRequest("The 'page' parameter must be greater than 0.");
            }

            if (limit <= 0)
            {
                _logger.LogWarning("Invalid limit parameter: {Limit}. It must be greater than 0.", limit);
                return BadRequest("The 'limit' parameter must be greater than 0.");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found.");
            }

            var userId = int.Parse(userIdClaim.Value);

            try
            {
                
                var pagedResult = await _toDoItemService.GetItemsByUserIdAsync(userId, page, limit, filter, sortBy);
                return Ok(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving to-do items.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the to-do items.");
            }
        }

        /// <summary>
        /// Gets a to-do item from the list by its ID.
        /// </summary>
        /// <param name="itemId">ID of the to-do item.</param>
        /// <returns>A response with the found item or a 404 status if not found.</returns>
        [HttpGet("{itemId}")]
        public async Task<IActionResult> GetToDoItemById(int itemId)
        {

            try
            {
                var item = await _toDoItemService.GetItemByIdAsync(itemId);
                if (item == null)
                {
                    _logger.LogInformation("To-do item with ID {ItemId} not found.", itemId);
                    return NotFound();
                }

                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving to-do item with ID {ItemId}.", itemId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the to-do item.");
            }
        }

        /// <summary>
        /// Creates a new item in the to-do list.
        /// </summary>
        /// <param name="newItem">Details of the new to-do item.</param>
        /// <returns>A response with the created item, or a 400 status if the request is invalid.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateToDoItem(ToDoItemRequest newItem)
        {
            // Model validation
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state is invalid when creating a new to-do item.");
                return BadRequest(ModelState);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found.");
            }

            var userId = int.Parse(userIdClaim.Value);

            try
            {
                var toDoItemCreated = await _toDoItemService.AddItemAsync(userId, newItem);
                return CreatedAtAction(nameof(GetToDoItemById), new { itemId = toDoItemCreated.ToDoItemId }, toDoItemCreated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating a new to-do item.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the to-do item.");
            }
        }

        /// <summary>
        /// Updates an existing item in the to-do list.
        /// </summary>
        /// <param name="itemId">ID of the item to update.</param>
        /// <param name="itemRequest">Updated details of the item.</param>
        /// <returns>A response with the updated item, or a 404 status if not found.</returns>
        [HttpPut("{itemId}")]
        public async Task<IActionResult> UpdateToDoItem(int itemId, ToDoItemRequest itemRequest)
        {
            // Model validation
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state is invalid when updating to-do item with ID {ItemId}.", itemId);
                return BadRequest(ModelState);
            }

            try
            {
                var itemUpdated = await _toDoItemService.UpdateItemAsync(itemId, itemRequest);
                if (itemUpdated == null)
                {
                    _logger.LogInformation("To-do item with ID {ItemId} not found for update.", itemId);
                    return NotFound();
                }

                return Ok(itemUpdated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating to-do item with ID {ItemId}.", itemId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the to-do item.");
            }
        }

        /// <summary>
        /// Deletes an item from the to-do list.
        /// </summary>
        /// <param name="itemId">ID of the item to delete.</param>
        /// <returns>A response with a 204 status if deletion is successful, or a 404 status if not found.</returns>
        [HttpDelete("{itemId}")]
        public async Task<IActionResult> DeleteToDoItem(int itemId)
        {

            try
            {
                var success = await _toDoItemService.DeleteItemAsync(itemId);
                if (!success)
                {
                    _logger.LogInformation("To-do item with ID {ItemId} not found for deletion.", itemId);
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting to-do item with ID {ItemId}.", itemId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the to-do item.");
            }
        }
    }
}


