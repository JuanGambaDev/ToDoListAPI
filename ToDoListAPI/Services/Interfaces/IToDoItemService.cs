using ToDoListAPI.DTOs;
using ToDoListAPI.Models;

namespace ToDoListAPI.Services;

public interface IToDoItemService 
{
    Task<IEnumerable<ToDoItem>> GetItemsAsync();
    Task<ToDoItem> GetItemByIdAsync(int itemId);
    Task<ToDoItem> AddItemAsync(ToDoItemRequest newItem);
    Task<ToDoItem> UpdateItemAsync(int id, ToDoItemRequest item);
}