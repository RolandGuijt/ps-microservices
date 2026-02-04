using GloboTicket.Services.EventCatalog.DbContexts;
using GloboTicket.Services.EventCatalog.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GloboTicket.Services.EventCatalog.Repositories;

public class CategoryRepository(EventCatalogDbContext eventCatalogDbContext): ICategoryRepository
{
    public async Task<IEnumerable<Category>> GetAllCategories()
    {
        return await eventCatalogDbContext.Categories.ToListAsync();
    }

    public async Task<Category> GetCategoryById(string categoryId)
    {
        return await eventCatalogDbContext.Categories.Where(x => x.CategoryId.ToString() == categoryId).FirstOrDefaultAsync();
    }
}
