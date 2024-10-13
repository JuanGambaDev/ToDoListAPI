using ToDoListAPI.DTOs;
using ToDoListAPI.Models;

namespace ToDoListAPI.Services;

public interface IToDoItemService 
{
    Task<PagedResult<ToDoItem>> GetItemsAsync(int page, int limit, string filter, string sortBy);
    Task<ToDoItem> GetItemByIdAsync(int itemId);
    Task<ToDoItem> AddItemAsync(ToDoItemRequest newItem);
    Task<ToDoItem> UpdateItemAsync(int id, ToDoItemRequest item);
    Task<bool> DeleteItemAsync(int itemId);
}