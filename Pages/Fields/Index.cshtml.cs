using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EavWebApp.Data;
using EavWebApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EavWebApp.Pages.Fields
{
	public class IndexModel : PageModel
	{
		private readonly ApplicationDbContext _context;
		public IndexModel(ApplicationDbContext context) => _context = context;

		public IList<Field> Fields { get; set; } = new List<Field>();

		public async Task OnGetAsync()
		{
			Fields = await _context.Fields
								   .Include(f => f.Table)
								   .ToListAsync();
		}
	}
}
