using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EavWebApp.Data;
using EavWebApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EavWebApp.Pages.Tables
{
	public class TableEditModel : PageModel
	{
		private readonly ApplicationDbContext _context;

		public TableEditModel(ApplicationDbContext context)
		{
			_context = context;
		}

		[BindProperty]
		public EVATable Table { get; set; } = new();

		public List<Field> Fields { get; set; } = new();
		public Dictionary<int, List<EVAValue>> ValuesByObjectId { get; set; } = new();
		public Dictionary<int, Dictionary<int, string>> KeyReferences { get; set; } = new();
		public Dictionary<int, List<KeyValuePair<int, string>>> KeyOptions { get; set; } = new();

		public async Task<IActionResult> OnGetAsync(int id)
		{
			Table = await _context.Tables
				.Include(t => t.Fields)
				.FirstOrDefaultAsync(t => t.Id == id);

			if (Table == null)
				return NotFound();

			Fields = Table.Fields.ToList();

			var values = await _context.Values
				.Where(v => v.TableId == id)
				.Include(v => v.Field)
				.ToListAsync();

			ValuesByObjectId = values
				.GroupBy(v => v.ObjectId)
				.ToDictionary(g => g.Key, g => g.ToList());

			// Загружаем связанные значения для key полей (как в JS)
			await LoadKeyReferences(values);
			await LoadKeyOptions();

			return Page();
		}

		private async Task LoadKeyReferences(List<EVAValue> values)
		{
			KeyReferences = new Dictionary<int, Dictionary<int, string>>();

			foreach (var keyField in Fields.Where(f => f.FieldType == "key" && f.IdKey.HasValue))
			{
				var fieldReferences = new Dictionary<int, string>();

				// Получаем все значения этого key поля
				var keyValues = values
					.Where(v => v.FieldId == keyField.Id && !string.IsNullOrEmpty(v.Val))
					.Select(v => v.Val)
					.Distinct()
					.ToList();

				foreach (var keyValue in keyValues)
				{
					if (int.TryParse(keyValue, out int referenceObjectId))
					{
						// Ищем display значение из связанного поля (как в JS)
						var displayValue = await GetDisplayValue(keyField.IdKey.Value, referenceObjectId);
						if (!string.IsNullOrEmpty(displayValue))
						{
							fieldReferences[referenceObjectId] = displayValue;
						}
						else
						{
							fieldReferences[referenceObjectId] = $"ID {referenceObjectId}";
						}
					}
				}
				KeyReferences[keyField.Id] = fieldReferences;
			}
		}

		private async Task<string> GetDisplayValue(int fieldId, int objectId)
		{
			var displayValue = await _context.Values
				.FirstOrDefaultAsync(v => v.FieldId == fieldId && v.ObjectId == objectId);

			return displayValue?.Val;
		}

		private async Task LoadKeyOptions()
		{
			KeyOptions = new Dictionary<int, List<KeyValuePair<int, string>>>();

			foreach (var keyField in Fields.Where(f => f.FieldType == "key" && f.IdKey.HasValue))
			{
				var options = await GetFieldOptions(keyField.IdKey.Value);
				KeyOptions[keyField.Id] = options;
			}
		}

		private async Task<List<KeyValuePair<int, string>>> GetFieldOptions(int fieldId)
		{
			var options = new List<KeyValuePair<int, string>>();

			// Получаем все уникальные значения поля (группируем по object_id как в JS)
			var records = await _context.Values
				.Where(v => v.FieldId == fieldId)
				.GroupBy(v => v.ObjectId)
				.Select(g => new { ObjectId = g.Key, DisplayValue = g.First().Val })
				.ToListAsync();

			foreach (var record in records)
			{
				options.Add(new KeyValuePair<int, string>(record.ObjectId, record.DisplayValue));
			}

			return options;
		}

		public async Task<IActionResult> OnPostAddValuesAsync(int id, int[] myIds, string[] myParams)
		{
			if (myIds == null || myParams == null)
				return RedirectToPage(new { id });

			int newObjectId = await GetNextObjectId(id);

			for (int i = 0; i < myIds.Length; i++)
			{
				_context.Values.Add(new EVAValue
				{
					TableId = id,
					FieldId = myIds[i],
					Val = myParams[i],
					ObjectId = newObjectId
				});
			}

			await _context.SaveChangesAsync();
			return RedirectToPage(new { id });
		}

		private async Task<int> GetNextObjectId(int tableId)
		{
			var existing = await _context.Values
				.Where(v => v.TableId == tableId)
				.ToListAsync();

			return existing.Any() ? existing.Max(v => v.ObjectId) + 1 : 1;
		}
	}
}