using ToDoListAPI.Models;

namespace ToDoListAPI.Repositories;

public interface IToDoItemRepository {

    Task<ToDoItem> AddItemAsync (ToDoItem item);

    Task<ToDoItem> GetItemByIdAsync (int itemId);

    Task<IQueryable<ToDoItem>> GetItemsByUserIdAsync(int userId);

    Task<ToDoItem> UpdateItemAsync (ToDoItem item);

    Task DeleteItemAsync (int itemId);
}