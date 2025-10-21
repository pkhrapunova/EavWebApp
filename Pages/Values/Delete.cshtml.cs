using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EavWebApp.Data;
using EavWebApp.Models;
using System.Threading.Tasks;

namespace EavWebApp.Pages.Values
{
	public class DeleteModel : PageModel
	{
		private readonly ApplicationDbContext _context;

		public DeleteModel(ApplicationDbContext context)
		{
			_context = context;
		}

		[BindProperty]
		public EVAValue Value { get; set; } = default!;

		// GET: подгрузка записи для подтверждения удаления
		public async Task<IActionResult> OnGetAsync(int id)
		{
			Value = await _context.Values.FindAsync(id);

			if (Value == null)
			{
				return NotFound();
			}

			return Page();
		}

		// POST: удаление записи
		public async Task<IActionResult> OnPostAsync(int id)
		{
			var valueToDelete = await _context.Values.FindAsync(id);

			if (valueToDelete != null)
			{
				_context.Values.Remove(valueToDelete);
				await _context.SaveChangesAsync();
			}

			return RedirectToPage("Index");
		}
	}
}
