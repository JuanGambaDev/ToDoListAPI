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

        public async Task<PagedResult<ToDoItem>> GetItemsByUserIdAsync(int userId,int page, int limit, string filter, string sortBy)
        {
            if (page <= 0 || limit <= 0)
            {
                _logger.LogWarning("Invalid pagination parameters: Page {Page}, Limit {Limit}.", page, limit);
                throw new ArgumentException("Page and limit must be greater than zero.");
            }

            try
            {
                // Esperar el resultado de la tarea
                var query = await _toDoItemRepository.GetItemsByUserIdAsync(userId);

                // Filter
                if (!string.IsNullOrEmpty(filter))
                {
                    if (int.TryParse(filter, out var id))
                    {
                        query = query.Where(item => item.ToDoItemId == id);
                    }
                    else
                    {
                        query = query.Where(item => item.Title.Contains(filter) || item.Description.Contains(filter));
                    }
                }

                // OrderBy
                if (!string.IsNullOrEmpty(sortBy))
                {
                    query = sortBy.ToLower() switch
                    {
                        "title" => query.OrderBy(item => item.Title),
                        "description" => query.OrderBy(item => item.Description),
                        "id" => query.OrderBy(item => item.ToDoItemId),
                        _ => throw new ArgumentException($"Invalid sort field: {sortBy}. Allowed values are 'title', 'description', or 'id'.")
                    };
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
            if (itemId <= 0)
            {
                _logger.LogWarning("Invalid ID provided for fetching to-do item: {ItemId}.", itemId);
                throw new ArgumentException("Item ID must be greater than zero.", nameof(itemId));
            }

            try
            {
                var item = await _toDoItemRepository.GetItemByIdAsync(itemId);
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

        public async Task<ToDoItem> AddItemAsync(int userId,ToDoItemRequest newItem)
        {
            if (newItem == null)
            {
                _logger.LogWarning("Attempted to add a null to-do item.");
                throw new ArgumentNullException(nameof(newItem), "New item cannot be null.");
            }

            try
            {
                var newToDoItem = new ToDoItem
                {
                    Title = newItem.Title,
                    Description = newItem.Description,
                    UserId = userId
                };

                return await _toDoItemRepository.AddItemAsync(newToDoItem);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error occurred while adding a new to-do item.");
                throw new Exception("Database error occurred while adding the to-do item.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while adding a new to-do item.");
                throw new Exception("An unexpected error occurred while adding the to-do item.", ex);
            }
        }

        public async Task<ToDoItem> UpdateItemAsync(int itemId, ToDoItemRequest itemRequest)
        {
            if (itemRequest == null)
            {
                _logger.LogWarning("Attempted to update a null to-do item.");
                throw new ArgumentNullException(nameof(itemRequest), "Item request cannot be null.");
            }

            try
            {
                var existingItem = await _toDoItemRepository.GetItemByIdAsync(itemId);
                if (existingItem == null)
                {
                    _logger.LogInformation("To-do item with ID {ItemId} not found for update.", itemId);
                    return null;
                }

                existingItem.Title = itemRequest.Title;
                existingItem.Description = itemRequest.Description;

                return await _toDoItemRepository.UpdateItemAsync(existingItem);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error occurred while updating to-do item with ID {ItemId}.", itemId);
                throw new Exception("Concurrency error occurred while updating the to-do item.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating to-do item with ID {ItemId}.", itemId);
                throw new Exception("Error updating the to-do item.", ex);
            }
        }

        public async Task<bool> DeleteItemAsync(int itemId)
        {
            if (itemId <= 0)
            {
                _logger.LogWarning("Invalid ID provided for deleting to-do item: {ItemId}.", itemId);
                throw new ArgumentException("Item ID must be greater than zero.", nameof(itemId));
            }

            try
            {
                var existingItem = await _toDoItemRepository.GetItemByIdAsync(itemId);
                if (existingItem == null)
                {
                    _logger.LogInformation("To-do item with ID {ItemId} not found for deletion.", itemId);
                    return false;
                }

                await _toDoItemRepository.DeleteItemAsync(itemId);
                return true;
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

