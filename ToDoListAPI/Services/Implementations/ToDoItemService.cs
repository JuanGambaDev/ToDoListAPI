using ToDoListAPI.Models;
using ToDoListAPI.DTOs;
using ToDoListAPI.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace ToDoListAPI.Services
{
    public class ToDoItemService : IToDoItemService
    {
        private readonly IToDoItemRepository _toDoItemRepository;
        private readonly ILogger<ToDoItemService> _logger;

        public ToDoItemService(IToDoItemRepository toDoItemRepository, ILogger<ToDoItemService> logger)
        {
            _toDoItemRepository = toDoItemRepository;
            _logger = logger;
        }

        public async Task<PagedResult<ToDoItem>> GetItemsAsync(int page, int limit, string filter, string sortBy)
        {
            try
            {
                var query = await _toDoItemRepository.GetItemsAsync();

                // Filter
                if (!string.IsNullOrEmpty(filter))
                {
                    // Try convert the filter to an int for filter by Id 
                    if (int.TryParse(filter, out var id))
                    {
                        query = query.Where(item => item.ToDoItemId == id);
                    }
                    else
                    {
                        // filter by title or description if isn't an Id 
                        query = query.Where(item => item.Title.Contains(filter) || item.Description.Contains(filter));
                    }
                }

                // OrderBy
                if (!string.IsNullOrEmpty(sortBy))
                {
                    switch (sortBy.ToLower())
                    {
                        case "title":
                            query = query.OrderBy(item => item.Title);
                            break;
                        case "description":
                            query = query.OrderBy(item => item.Description);
                            break;
                        case "id":
                            query = query.OrderBy(item => item.ToDoItemId);
                            break;
                        default:
                            throw new ArgumentException($"Invalid sort field: {sortBy}. Allowed values are 'title', 'description', or 'id'.");
                    }
                }

                // Pagination
                var totalItems = await query.CountAsync();
                var items = await query.Skip((page - 1) * limit).Take(limit).ToListAsync();

                return new PagedResult<ToDoItem>
                {
                    Data = items,
                    Page = page,
                    Limit = limit,
                    Total = totalItems
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching to-do items.");
                throw new Exception("Error fetching to-do items.", ex);
            }
        }

        public async Task<ToDoItem> GetItemByIdAsync(int itemId)
        {
            try
            {
                return await _toDoItemRepository.GetItemByIdAsync(itemId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching to-do item with ID {ItemId}.", itemId);
                throw new Exception("Error fetching the to-do item.", ex);
            }
        }

        public async Task<ToDoItem> AddItemAsync(ToDoItemRequest newItem)
        {
            try
            {
                var newToDoItem = new ToDoItem
                {
                    Title = newItem.Title,
                    Description = newItem.Description,
                };

                return await _toDoItemRepository.AddItemAsync(newToDoItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding a new to-do item.");
                throw new Exception("Error adding the to-do item.", ex);
            }
        }

        public async Task<ToDoItem> UpdateItemAsync(int itemId, ToDoItemRequest itemRequest)
        {
            try
            {
                var existingItem = await _toDoItemRepository.GetItemByIdAsync(itemId);

                if (existingItem == null)
                {
                    return null;
                }

                existingItem.Title = itemRequest.Title;
                existingItem.Description = itemRequest.Description;

                return await _toDoItemRepository.UpdateItemAsync(existingItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating to-do item with ID {ItemId}.", itemId);
                throw new Exception("Error updating the to-do item.", ex);
            }
        }

        public async Task<bool> DeleteItemAsync(int itemId)
        {
            try
            {
                var existingItem = await _toDoItemRepository.GetItemByIdAsync(itemId);

                if (existingItem == null)
                {
                    return false;
                }
                
                await _toDoItemRepository.DeleteItemAsync(itemId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting to-do item with ID {ItemId}.", itemId);
                throw new Exception("Error deleting the to-do item.", ex);
            }
        }
    }
}
