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
	public class CreateModel : PageModel
	{
		private readonly ApplicationDbContext _context;
		public CreateModel(ApplicationDbContext context) => _context = context;

		[BindProperty]
		public EVAValue Value { get; set; } = new();

		public SelectList FieldSelectList { get; set; } = default!;
		public SelectList TableSelectList { get; set; } = default!;

		public async Task OnGetAsync()
		{
			var fields = await _context.Fields.ToListAsync();
			FieldSelectList = new SelectList(fields, "Id", "Name");

			var tables = await _context.Tables.ToListAsync();
			TableSelectList = new SelectList(tables, "Id", "Name");
		}

		public async Task<IActionResult> OnPostAsync()
		{
			if (!ModelState.IsValid)
			{
				var fields = await _context.Fields.ToListAsync();
				FieldSelectList = new SelectList(fields, "Id", "Name");

				var tables = await _context.Tables.ToListAsync();
				TableSelectList = new SelectList(tables, "Id", "Name");

				return Page();
			}

			_context.Values.Add(Value);
			await _context.SaveChangesAsync();

			return RedirectToPage("Index");
		}
	}
}
