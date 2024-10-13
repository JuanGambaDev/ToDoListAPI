using ToDoListAPI.DTOs;
using ToDoListAPI.Models;

namespace ToDoListAPI.Services;

public interface IToDoItemService {
    Task<ToDoItem> AddItemAsync(ToDoItemRequest newItem);

    Task<ToDoItem> GetItemByIdAsync(int itemId);

    Task<IEnumerable<ToDoItem>> GetItemsAsync();
}