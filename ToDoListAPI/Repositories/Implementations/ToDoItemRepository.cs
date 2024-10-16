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
            if (newItem == null)
            {
                _logger.LogWarning("Attempted to add a null to-do item.");
                throw new ArgumentNullException(nameof(newItem), "New item cannot be null.");
            }

            try
            {
                await _context.ToDoItems.AddAsync(newItem);
                await _context.SaveChangesAsync();
                return newItem;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error occurred while adding new to-do item.");
                throw new Exception("Database error occurred while adding the new to-do item.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while adding new to-do item.");
                throw new Exception("An unexpected error occurred while adding the new to-do item.", ex);
            }
        }

        public async Task<ToDoItem> GetItemByIdAsync(int itemId)
        {
            if (itemId <= 0)
            {
                _logger.LogWarning("Invalid ID provided for fetching to-do item: {ItemId}.", itemId);
                throw new ArgumentException("Item ID must be greater than zero.", nameof(itemId));
            }

            try
            {
                var item = await _context.ToDoItems.FirstOrDefaultAsync(i => i.ToDoItemId == itemId);
                if (item == null)
                {
                    _logger.LogInformation("To-do item with ID {ItemId} not found.", itemId);
                }
                return item;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching to-do item with ID {ItemId}.", itemId);
                throw new Exception("Error fetching the to-do item.", ex);
            }
        }

        public async Task<IQueryable<ToDoItem>> GetItemsByUserIdAsync(int userId)
        {
            try
            {
                return _context.ToDoItems.Where(i => i.UserId == userId).AsQueryable();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching to-do items.");
                throw new Exception("Error fetching to-do items.", ex);
            }
        }

        public async Task<ToDoItem> UpdateItemAsync(ToDoItem item)
        {
            if (item == null)
            {
                _logger.LogWarning("Attempted to update a null to-do item.");
                throw new ArgumentNullException(nameof(item), "Item cannot be null.");
            }

            try
            {
                _context.ToDoItems.Update(item);
                await _context.SaveChangesAsync();
                return item;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error occurred while updating to-do item with ID {ItemId}.", item.ToDoItemId);
                throw new Exception("Concurrency error occurred while updating the to-do item.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating to-do item with ID {ItemId}.", item.ToDoItemId);
                throw new Exception("Error updating the to-do item.", ex);
            }
        }

        public async Task DeleteItemAsync(int itemId)
        {
            if (itemId <= 0)
            {
                _logger.LogWarning("Invalid ID provided for deleting to-do item: {ItemId}.", itemId);
                throw new ArgumentException("Item ID must be greater than zero.", nameof(itemId));
            }

            try
            {
                var item = await _context.ToDoItems.FindAsync(itemId);
                if (item != null)
                {
                    _context.ToDoItems.Remove(item);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    _logger.LogInformation("To-do item with ID {ItemId} not found for deletion.", itemId);
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error occurred while deleting to-do item with ID {ItemId}.", itemId);
                throw new Exception("Database error occurred while deleting the to-do item.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting to-do item with ID {ItemId}.", itemId);
                throw new Exception("Error deleting the to-do item.", ex);
            }
        }
    }
}

