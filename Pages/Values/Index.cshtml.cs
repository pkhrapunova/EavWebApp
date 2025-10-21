using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EavWebApp.Data;
using EavWebApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EavWebApp.Pages.Values
{
	public class IndexModel : PageModel
	{
		private readonly ApplicationDbContext _context;
		public IndexModel(ApplicationDbContext context) => _context = context;

		public IList<EVAValue> Values { get; set; } = new List<EVAValue>();

		public async Task OnGetAsync()
		{
			Values = await _context.Values
								   .Include(v => v.Field)
								   .Include(v => v.Table)
								   .ToListAsync();
		}
	}
}
