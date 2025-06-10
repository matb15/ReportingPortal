using System.Collections.Generic;
using System.Linq;
using Models;
using ReportingPortalServer.Data;

public interface ICategoryService
{
    IEnumerable<Category> GetAll(ApplicationDbContext context);
    Category? GetById(int id, ApplicationDbContext context);
    Category Add(Category category, ApplicationDbContext context);
}

public class CategoryService : ICategoryService
{
    public IEnumerable<Category> GetAll(ApplicationDbContext context)
        => context.Categories.ToList();

    public Category? GetById(int id, ApplicationDbContext context)
        => context.Categories.FirstOrDefault(c => c.Id == id);

    public Category Add(Category category, ApplicationDbContext context)
    {
        context.Categories.Add(category);
        context.SaveChanges();
        return category;
    }
}