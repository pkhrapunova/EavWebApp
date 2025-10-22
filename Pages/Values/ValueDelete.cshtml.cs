using System.Linq;
using System.Threading.Tasks;
using EavWebApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EavWebApp.Pages.Values
{
	public class ValueDeleteModel : PageModel
	{
		private readonly ApplicationDbContext _context;

		public ValueDeleteModel(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<IActionResult> OnGetAsync(int tableId, int objectId)
		{
			var values = await _context.Values
				.Where(v => v.TableId == tableId && v.ObjectId == objectId)
				.ToListAsync();

			if (!values.Any())
				return NotFound();

			_context.Values.RemoveRange(values);
			await _context.SaveChangesAsync();

			return RedirectToPage("/Tables/TableEdit", new { id = tableId });
		}
	}
}