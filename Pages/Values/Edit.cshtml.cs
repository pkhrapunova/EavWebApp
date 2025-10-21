using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EavWebApp.Data;
using EavWebApp.Models;
using System.Threading.Tasks;
using System.Linq;

namespace EavWebApp.Pages.Values
{
	public class EditModel : PageModel
	{
		private readonly ApplicationDbContext _context;
		public EditModel(ApplicationDbContext context) => _context = context;

		[BindProperty]
		public EVAValue Value { get; set; } = new();

		public SelectList FieldSelectList { get; set; } = default!;
		public SelectList TableSelectList { get; set; } = default!;

		public async Task<IActionResult> OnGetAsync(int id)
		{
			Value = await _context.Values.FindAsync(id);
			if (Value == null) return NotFound();

			var fields = await _context.Fields.ToListAsync();
			FieldSelectList = new SelectList(fields, "Id", "Name", Value.FieldId);

			var tables = await _context.Tables.ToListAsync();
			TableSelectList = new SelectList(tables, "Id", "Name", Value.TableId);

			return Page();
		}

		public async Task<IActionResult> OnPostAsync()
		{
			if (!ModelState.IsValid)
			{
				var fields = await _context.Fields.ToListAsync();
				FieldSelectList = new SelectList(fields, "Id", "Name", Value.FieldId);

				var tables = await _context.Tables.ToListAsync();
				TableSelectList = new SelectList(tables, "Id", "Name", Value.TableId);

				return Page();
			}

			_context.Attach(Value).State = EntityState.Modified;
			await _context.SaveChangesAsync();

			return RedirectToPage("Index");
		}
	}
}
