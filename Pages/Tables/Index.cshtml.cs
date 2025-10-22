using System.Collections.Generic;
using System.Threading.Tasks;
using EavWebApp.Data;
using EavWebApp.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EavWebApp.Pages.Tables
{
	public class IndexModel : PageModel
	{
		private readonly ApplicationDbContext _context;

		public IndexModel(ApplicationDbContext context)
		{
			_context = context;
		}

		public IList<EVATable> Tables { get; set; } = new List<EVATable>();

		public async Task OnGetAsync()
		{
			Tables = await _context.Tables
								   .AsNoTracking()
								   .ToListAsync();
		}
	}
}
