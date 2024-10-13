using ToDoListAPI.Models;

namespace ToDoListAPI.Repositories;

public interface IToDoItemRepository {

    Task<ToDoItem> AddItemAsync (ToDoItem item);

    Task<ToDoItem> GetItemByIdAsync (int itemId);

    Task<IQueryable<ToDoItem>> GetItemsAsync ();

    Task<ToDoItem> UpdateItemAsync (ToDoItem item);

    Task DeleteItemAsync (int itemId);
}