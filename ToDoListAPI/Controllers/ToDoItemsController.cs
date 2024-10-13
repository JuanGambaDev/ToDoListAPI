using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ToDoListAPI.DTOs;
using ToDoListAPI.Models;
using ToDoListAPI.Services;

namespace ToDoListAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ToDoItemsController : ControllerBase
{
    private readonly IToDoItemService _toDoItemService;

    public ToDoItemsController(IToDoItemService toDoItemService){
        _toDoItemService = toDoItemService;
    }

    [HttpGet("{itemId}")]
    public async Task<IActionResult> GetToDoItemById (int itemId)
    {
        var item = await _toDoItemService.GetItemByIdAsync(itemId);
        
        if (item == null)
        {
            return NotFound(); // Devuelve 404 si no se encuentra el recurso
        }

        return Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> CreateToDoItem(ToDoItemRequest newItem){

        if (!ModelState.IsValid)
        {
        return BadRequest(ModelState); // Devuelve el estado del modelo si no es válido
        }

        var toDoItemCreated = await _toDoItemService.AddItemAsync(newItem);

        return CreatedAtAction(nameof(GetToDoItemById), new { id = toDoItemCreated.ToDoItemId }, toDoItemCreated);
    }

    [HttpPut("itemId")]
    public async Task<IActionResult> UpdateToDoItem (int itemId, ToDoItemRequest itemRequest){
        if (!ModelState.IsValid)
        {
        return BadRequest(ModelState); // Devuelve el estado del modelo si no es válido
        }
        
        var itemUpdated = await _toDoItemService.UpdateItemAsync(itemId, itemRequest);

        if (itemUpdated == null)
        {
            return NotFound(); // Devuelve 404 si no se encuentra el recurso
        }

        return Ok(itemUpdated);
    }
}

