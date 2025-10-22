using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EavWebApp.Pages
{
	public class IndexModel : PageModel
	{
		public IActionResult OnGet()
		{
			return RedirectToPage("/Tables/Index");
		}
	}
}
