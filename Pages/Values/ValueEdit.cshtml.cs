using System.Linq;
using System.Threading.Tasks;
using EavWebApp.Data;
using EavWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EavWebApp.Pages.Values
{
	public class ValueEditModel : PageModel
	{
		private readonly ApplicationDbContext _context;

		public ValueEditModel(ApplicationDbContext context)
		{
			_context = context;
		}

		[BindProperty]
		public List<EVAValue> Values { get; set; } = new List<EVAValue>();

		public EVATable TableInfo { get; set; }
		public List<Field> Fields { get; set; } = new List<Field>();
		public Dictionary<int, List<KeyValuePair<int, string>>> KeyOptions { get; set; } = new();

		public async Task<IActionResult> OnGetAsync(int tableId, int objectId)
		{
			await LoadTableData(tableId);
			await LoadValues(tableId, objectId);
			await LoadKeyOptions();

			return Page();
		}

		private async Task LoadTableData(int tableId)
		{
			TableInfo = await _context.Tables
				.Include(t => t.Fields) 
				.FirstOrDefaultAsync(t => t.Id == tableId);

			if (TableInfo != null)
			{
				Fields = TableInfo.Fields.ToList();
			}
		}

		private async Task LoadValues(int tableId, int objectId)
		{
			var existingValues = await _context.Values
				.Where(v => v.TableId == tableId && v.ObjectId == objectId)
				.ToListAsync();

			Values = new List<EVAValue>();
			foreach (var field in Fields)
			{
				var value = existingValues.FirstOrDefault(v => v.FieldId == field.Id);
				if (value == null)
				{
					value = new EVAValue
					{
						TableId = tableId,
						FieldId = field.Id,
						ObjectId = objectId,
						Val = ""
					};
				}
				Values.Add(value);
			}
		}

		private async Task LoadKeyOptions()
		{
			KeyOptions = new Dictionary<int, List<KeyValuePair<int, string>>>();

			foreach (var keyField in Fields.Where(f => f.FieldType == "key" && f.IdKey.HasValue))
			{
				var referenceTable = await _context.Tables
					.Include(t => t.Fields)
					.FirstOrDefaultAsync(t => t.Id == keyField.IdKey.Value);

				if (referenceTable != null)
				{
					var displayField = referenceTable.Fields.FirstOrDefault();
					if (displayField != null)
					{
						var records = await _context.Values
							.Where(v => v.TableId == referenceTable.Id && v.FieldId == displayField.Id)
							.GroupBy(v => v.ObjectId)
							.Select(g => new { ObjectId = g.Key, DisplayValue = g.First().Val })
							.ToListAsync();

						var options = records.Select(r => new KeyValuePair<int, string>(r.ObjectId, $"{r.DisplayValue}")).ToList();
						KeyOptions[keyField.Id] = options;
					}
				}
			}
		}

		public async Task<IActionResult> OnPostAsync(int tableId, int objectId)
		{
			if (!ModelState.IsValid)
			{
				await LoadTableData(tableId);
				await LoadKeyOptions();
				return Page();
			}

			foreach (var value in Values)
			{
				var existingValue = await _context.Values
					.FirstOrDefaultAsync(v => v.TableId == tableId &&
											v.ObjectId == objectId &&
											v.FieldId == value.FieldId);

				if (existingValue != null)
				{
					existingValue.Val = value.Val;
				}
				else
				{
					value.TableId = tableId;
					value.ObjectId = objectId;
					_context.Values.Add(value);
				}
			}

			await _context.SaveChangesAsync();
			return RedirectToPage("/Tables/TableEdit", new { id = tableId });
		}
	}
}