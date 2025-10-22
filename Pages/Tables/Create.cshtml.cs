using System.Threading.Tasks;
using EavWebApp.Data;
using EavWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace EavWebApp.Pages.Tables
{
	public class CreateModel : PageModel
	{
		private readonly ApplicationDbContext _context;

		public CreateModel(ApplicationDbContext context)
		{
			_context = context;
		}

		[BindProperty]
		public EVATable Table { get; set; } = new();

		public async Task<IActionResult> OnPostAsync()
		{
			if (!ModelState.IsValid) return Page();

			_context.Tables.Add(Table);
			await _context.SaveChangesAsync();

			return RedirectToPage("Index");
		}
	}
}
