using Microsoft.EntityFrameworkCore;
using ToDoListAPI.Data;
using ToDoListAPI.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ToDoListAPI.Repositories
{
    public class ToDoItemRepository : IToDoItemRepository
    {
        private readonly ToDoListContext _context;
        private readonly ILogger<ToDoItemRepository> _logger;

        public ToDoItemRepository(ToDoListContext context, ILogger<ToDoItemRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ToDoItem> AddItemAsync(ToDoItem newItem)
        {
            try
            {
                await _context.ToDoItems.AddAsync(newItem);
                await _context.SaveChangesAsync();
                return newItem;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding new to-do item.");
                throw new Exception("Error adding new to-do item.", ex);
            }
        }

        public async Task<ToDoItem> GetItemByIdAsync(int itemId)
        {
            try
            {
                return await _context.ToDoItems.FirstOrDefaultAsync(i => i.ToDoItemId == itemId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching to-do item with ID {ItemId}.", itemId);
                throw new Exception("Error fetching the to-do item.", ex);
            }
        }

        public async Task<IQueryable<ToDoItem>> GetItemsAsync()
        {
            try
            {
                return _context.ToDoItems.AsQueryable();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching to-do items.");
                throw new Exception("Error fetching to-do items.", ex);
            }
        }

        public async Task<ToDoItem> UpdateItemAsync(ToDoItem item)
        {
            try
            {
                _context.ToDoItems.Update(item);
                await _context.SaveChangesAsync();
                return item;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating to-do item with ID {ItemId}.", item.ToDoItemId);
                throw new Exception("Error updating the to-do item.", ex);
            }
        }

        public async Task DeleteItemAsync(int itemId)
        {
            try
            {
                var item = await _context.ToDoItems.FindAsync(itemId);
                if (item != null)
                {
                    _context.ToDoItems.Remove(item);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting to-do item with ID {ItemId}.", itemId);
                throw new Exception("Error deleting the to-do item.", ex);
            }
        }
    }
}
