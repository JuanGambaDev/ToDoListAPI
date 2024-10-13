using Microsoft.EntityFrameworkCore;
using ToDoListAPI.Data;
using ToDoListAPI.Models;

namespace ToDoListAPI.Repositories;

public class ToDoItemRepository : IToDoItemRepository {

    private readonly ToDoListContext _context;

    public ToDoItemRepository (ToDoListContext context) {
        _context = context;
    }

    public async Task<ToDoItem> AddItemAsync (ToDoItem newItem) {
        await _context.toDoItems.AddAsync (newItem);
        await _context.SaveChangesAsync();
        return newItem;

    }

    public async Task<ToDoItem> GetItemByIdAsync (int itemId){
        return await _context.toDoItems.FirstOrDefaultAsync(i => i.ToDoItemId == itemId);
    }

    public async Task<IEnumerable<ToDoItem>> GetItemsAsync (){
        return await _context.toDoItems.ToListAsync();
    }

    public async Task<ToDoItem> UpdateItemAsync (ToDoItem item)
    {
        _context.toDoItems.Update(item);
        await _context.SaveChangesAsync();
        return item;
    }

}