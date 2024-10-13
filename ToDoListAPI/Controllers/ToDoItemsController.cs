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

    [HttpPost]
    public async Task<IActionResult> CreateToDoItem(ToDoItemRequest newItem){

        if (!ModelState.IsValid)
        {
        return BadRequest(ModelState); // Devuelve el estado del modelo si no es válido
        }

        var toDoItemCreated = await _toDoItemService.AddItemAsync(newItem);

        return CreatedAtAction(nameof(GetToDoItems), new { id = toDoItemCreated.ToDoItemId }, toDoItemCreated);
    }

    [HttpGet("{itemId}")]
    public async Task<IActionResult> GetToDoItems (int? itemId = null)
    {
        if (itemId.HasValue)
        {
            // Si se proporciona un ID, devuelve el elemento correspondiente
            var item = await _toDoItemService.GetItemByIdAsync(itemId.Value);
            
            if (item == null)
            {
                return NotFound(); // Devuelve 404 si no se encuentra el recurso
            }

            return Ok(item); // Devuelve el recurso encontrado
        }
        else
        {
            // Si no se proporciona un ID, devuelve la lista de todos los elementos
            var items = await _toDoItemService.GetItemsAsync();
            return Ok(items); // Devuelve la lista de elementos
        }

    }
}

