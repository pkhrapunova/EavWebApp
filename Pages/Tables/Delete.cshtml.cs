using System.Threading.Tasks;
using EavWebApp.Data;
using EavWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace EavWebApp.Pages.Tables
{
	public class DeleteModel : PageModel
	{
		private readonly ApplicationDbContext _context;
		public DeleteModel(ApplicationDbContext context) => _context = context;

		[BindProperty]
		public EVATable Table { get; set; } = new();

		public async Task<IActionResult> OnGetAsync(int id)
		{
			Table = await _context.Tables.FindAsync(id);
			if (Table == null) return NotFound();
			return Page();
		}

		public async Task<IActionResult> OnPostAsync()
		{
			var table = await _context.Tables.FindAsync(Table.Id);
			if (table != null)
			{
				// Удаляем связанные поля и значения
				var fields = _context.Fields.Where(f => f.TableId == table.Id);
				_context.Fields.RemoveRange(fields);

				var values = _context.Values.Where(v => v.TableId == table.Id);
				_context.Values.RemoveRange(values);

				_context.Tables.Remove(table);
				await _context.SaveChangesAsync();
			}
			return RedirectToPage("Index");
		}
	}
}
