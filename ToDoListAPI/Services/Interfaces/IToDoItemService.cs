using ToDoListAPI.DTOs;
using ToDoListAPI.Models;

namespace ToDoListAPI.Services;

public interface IToDoItemService 
{
    Task<PagedResult<ToDoItem>> GetItemsByUserIdAsync(int userId,int page, int limit, string filter, string sortBy);
    Task<ToDoItem> GetItemByIdAsync(int itemId);
    Task<ToDoItem> AddItemAsync(int userId,ToDoItemRequest newItem);
    Task<ToDoItem> UpdateItemAsync(int id, ToDoItemRequest item);
    Task<bool> DeleteItemAsync(int itemId);
}