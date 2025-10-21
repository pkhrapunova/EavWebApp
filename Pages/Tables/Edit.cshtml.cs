using System.Threading.Tasks;
using EavWebApp.Data;
using EavWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace EavWebApp.Pages.Tables
{
	public class EditModel : PageModel
	{
		private readonly ApplicationDbContext _context;
		public EditModel(ApplicationDbContext context) => _context = context;

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
			if (!ModelState.IsValid) return Page();

			_context.Attach(Table).State = EntityState.Modified;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!await _context.Tables.AnyAsync(t => t.Id == Table.Id))
					return NotFound();
				else
					throw;
			}

			return RedirectToPage("Index");
		}
	}
}
