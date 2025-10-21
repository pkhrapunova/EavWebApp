using System.Threading.Tasks;
using EavWebApp.Data;
using EavWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EavWebApp.Pages.Fields
{
	public class CreateModel : PageModel
	{
		private readonly ApplicationDbContext _context;
		public CreateModel(ApplicationDbContext context) => _context = context;

		[BindProperty]
		public Field Field { get; set; } = new();

		public SelectList TableSelectList { get; set; } = default!;

		public async Task OnGetAsync()
		{
			var tables = await _context.Tables.ToListAsync();
			TableSelectList = new SelectList(tables, "Id", "Name");
		}

		public async Task<IActionResult> OnPostAsync()
		{
			if (!ModelState.IsValid)
			{
				var tables = await _context.Tables.ToListAsync();
				TableSelectList = new SelectList(tables, "Id", "Name");
				return Page();
			}

			_context.Fields.Add(Field);
			await _context.SaveChangesAsync();

			return RedirectToPage("Index");
		}
	}
}
