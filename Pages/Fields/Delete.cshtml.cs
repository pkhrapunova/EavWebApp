using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EavWebApp.Data;
using EavWebApp.Models;
using System.Threading.Tasks;

namespace EavWebApp.Pages.Fields
{
	public class DeleteModel : PageModel
	{
		private readonly ApplicationDbContext _context;
		public DeleteModel(ApplicationDbContext context) => _context = context;

		[BindProperty]
		public Field Field { get; set; } = new();

		public async Task<IActionResult> OnGetAsync(int id)
		{
			Field = await _context.Fields.FindAsync(id);
			if (Field == null) return NotFound();
			return Page();
		}

		public async Task<IActionResult> OnPostAsync()
		{
			var field = await _context.Fields.FindAsync(Field.Id);
			if (field != null)
			{
				// Удаляем все связанные значения
				var values = _context.Values.Where(v => v.FieldId == field.Id);
				_context.Values.RemoveRange(values);

				_context.Fields.Remove(field);
				await _context.SaveChangesAsync();
			}

			return RedirectToPage("Index");
		}
	}
}
