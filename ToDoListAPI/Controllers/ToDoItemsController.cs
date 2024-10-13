using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using ToDoListAPI.DTOs;
using ToDoListAPI.Models;
using ToDoListAPI.Services;

namespace ToDoListAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToDoItemsController : ControllerBase
    {
        private readonly IToDoItemService _toDoItemService;
        private readonly ILogger<ToDoItemsController> _logger;

        public ToDoItemsController(IToDoItemService toDoItemService, ILogger<ToDoItemsController> logger)
        {
            _toDoItemService = toDoItemService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetToDoItems(int page = 1, int limit = 10, string filter = null, string sortBy = null)
        {
            try
            {
                var pagedResult = await _toDoItemService.GetItemsAsync(page, limit, filter, sortBy);
                return Ok(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving to-do items.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the to-do items.");
            }
        }

        [HttpGet("{itemId}")]
        public async Task<IActionResult> GetToDoItemById(int itemId)
        {
            try
            {
                var item = await _toDoItemService.GetItemByIdAsync(itemId);
                if (item == null)
                {
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

        [HttpPost]
        public async Task<IActionResult> CreateToDoItem(ToDoItemRequest newItem)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var toDoItemCreated = await _toDoItemService.AddItemAsync(newItem);
                return CreatedAtAction(nameof(GetToDoItemById), new { id = toDoItemCreated.ToDoItemId }, toDoItemCreated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating a new to-do item.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the to-do item.");
            }
        }

        [HttpPut("{itemId}")]
        public async Task<IActionResult> UpdateToDoItem(int itemId, ToDoItemRequest itemRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var itemUpdated = await _toDoItemService.UpdateItemAsync(itemId, itemRequest);
                if (itemUpdated == null)
                {
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

        [HttpDelete("{itemId}")]
        public async Task<IActionResult> DeleteToDoItem(int itemId)
        {
            try
            {
                var success = await _toDoItemService.DeleteItemAsync(itemId);
                if (!success)
                {
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

