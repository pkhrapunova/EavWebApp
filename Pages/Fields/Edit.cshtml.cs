using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EavWebApp.Data;
using EavWebApp.Models;
using System.Threading.Tasks;

namespace EavWebApp.Pages.Fields
{
	public class EditModel : PageModel
	{
		private readonly ApplicationDbContext _context;
		public EditModel(ApplicationDbContext context) => _context = context;

		[BindProperty]
		public Field Field { get; set; } = new();

		public SelectList TableSelectList { get; set; } = default!;

		public async Task<IActionResult> OnGetAsync(int id)
		{
			Field = await _context.Fields.FindAsync(id);
			if (Field == null) return NotFound();

			var tables = await _context.Tables.ToListAsync();
			TableSelectList = new SelectList(tables, "Id", "Name", Field.TableId);

			return Page();
		}

		public async Task<IActionResult> OnPostAsync()
		{
			if (!ModelState.IsValid)
			{
				var tables = await _context.Tables.ToListAsync();
				TableSelectList = new SelectList(tables, "Id", "Name", Field.TableId);
				return Page();
			}

			_context.Attach(Field).State = EntityState.Modified;
			await _context.SaveChangesAsync();

			return RedirectToPage("Index");
		}
	}
}
