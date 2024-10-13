using ToDoListAPI.Models;
using ToDoListAPI.DTOs;
using ToDoListAPI.Repositories;
using Microsoft.AspNetCore.Http.Features;

namespace ToDoListAPI.Services;

public class ToDoItemService : IToDoItemService {

    private readonly IToDoItemRepository _toDoItemRepository;

    public ToDoItemService(IToDoItemRepository toDoItemRepository) {
        _toDoItemRepository = toDoItemRepository;
    }

    public async Task<ToDoItem> AddItemAsync (ToDoItemRequest newItem){
        var newToDoItem = new ToDoItem 
        {
            Title = newItem.Title,
            Description = newItem.Description,
        };

        var createdItem = await _toDoItemRepository.AddItemAsync (newToDoItem);

        return createdItem;
    }

    public async Task<ToDoItem> GetItemByIdAsync(int itemId)
    {
        return await _toDoItemRepository.GetItemByIdAsync (itemId);
    }

    public async Task<IEnumerable<ToDoItem>> GetItemsAsync(){
        return await _toDoItemRepository.GetItemsAsync();
    }



}