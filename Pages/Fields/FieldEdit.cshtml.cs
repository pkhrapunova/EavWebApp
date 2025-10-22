using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EavWebApp.Data;
using EavWebApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EavWebApp.Pages.Fields
{
	public class FieldEditModel : PageModel
	{
		private readonly ApplicationDbContext _context;

		public FieldEditModel(ApplicationDbContext context)
		{
			_context = context;
		}

		[BindProperty]
		public EVATable Table { get; set; } = new();

		public List<Field> Fields { get; set; } = new();
		public List<EVATable> AllTables { get; set; } = new();

		[BindProperty]
		public Field NewField { get; set; } = new();

		// Поддержка редактирования: id поля может быть null
		public async Task<IActionResult> OnGetAsync(int tableId, int? id)
		{
			Table = await _context.Tables
				.Include(t => t.Fields)
				.FirstOrDefaultAsync(t => t.Id == tableId);

			if (Table == null)
				return NotFound();

			Fields = Table.Fields.ToList();
			AllTables = await _context.Tables.Where(t => t.Id != tableId).ToListAsync();

			if (id.HasValue)
			{
				var field = await _context.Fields.FindAsync(id.Value);
				if (field != null)
				{
					NewField = field; // заполняем форму редактирования
				}
			}

			return Page();
		}


		public async Task<IActionResult> OnPostAddFieldAsync(int tableId)
		{
			var table = await _context.Tables.FindAsync(tableId);
			if (table == null)
				return NotFound();

			if (NewField.FieldType == "key" && (NewField.IdKey == null || NewField.IdKey == 0))
			{
				ModelState.AddModelError("NewField.IdKey", "Для ключевого поля необходимо выбрать поле для отображения");
			}
			else if (NewField.FieldType != "key")
			{
				NewField.IdKey = null;
			}

			if (!ModelState.IsValid)
			{
				Table = await _context.Tables
					.Include(t => t.Fields)
					.FirstOrDefaultAsync(t => t.Id == tableId);
				Fields = Table.Fields.ToList();
				AllTables = await _context.Tables.Where(t => t.Id != tableId).ToListAsync();
				return Page();
			}

			NewField.TableId = tableId;
			_context.Fields.Add(NewField);
			await _context.SaveChangesAsync();

			return RedirectToPage(new { tableId });
		}

		public async Task<IActionResult> OnPostEditFieldAsync(int tableId)
		{
			var field = await _context.Fields.FindAsync(NewField.Id);
			if (field == null)
				return NotFound();

			field.Name = NewField.Name;
			field.FieldType = NewField.FieldType;
			field.IdKey = NewField.FieldType == "key" ? NewField.IdKey : null;

			await _context.SaveChangesAsync();

			return RedirectToPage(new { tableId });
		}

		public async Task<IActionResult> OnGetDeleteFieldAsync(int id)
		{
			var field = await _context.Fields
				.Include(f => f.Values)
				.FirstOrDefaultAsync(f => f.Id == id);

			if (field == null)
				return NotFound();

			_context.Values.RemoveRange(field.Values);
			_context.Fields.Remove(field);
			await _context.SaveChangesAsync();

			return RedirectToPage(new { tableId = field.TableId });
		}

		public async Task<JsonResult> OnGetTableFieldsAsync([FromQuery] int tableId)
		{
			var fields = await _context.Fields
				.Where(f => f.TableId == tableId)
				.Select(f => new { id = f.Id, name = f.Name })
				.ToListAsync();

			return new JsonResult(fields);
		}
	}
}
