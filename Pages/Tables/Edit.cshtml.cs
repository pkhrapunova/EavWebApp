using System.IO;
using EavWebApp.Data;
using EavWebApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EavWebApp.Pages.Tables
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public EditModel(ApplicationDbContext context) => _context = context;

        public EVATable Table { get; set; } = new();
        public List<Field> Fields { get; set; } = new();
        public Dictionary<int, List<KeyValuePair<int, string>>> KeyOptions { get; set; } = new();
        public Dictionary<int, Dictionary<int, string>> KeyReferences { get; set; } = new();
        public Dictionary<int, List<EVAValue>> ValuesByObjectId { get; set; } = new();

        [BindProperty] public Dictionary<int, string> TextValues { get; set; } = new();
        [BindProperty] public Dictionary<int, IFormFile> ImageFiles { get; set; } = new();
        [BindProperty] public Dictionary<int, int> KeyValues { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Console.WriteLine($"[OnGet] Loading table {id}");
            Table = await _context.Tables
                .Include(t => t.Fields)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (Table == null)
            {
                Console.WriteLine($"[OnGet] Table {id} not found!");
                return NotFound();
            }

            Fields = Table.Fields.ToList();
            Console.WriteLine($"[OnGet] Fields loaded: {Fields.Count}");

            // Загружаем существующие значения
            var values = await _context.Values.Where(v => v.TableId == id).ToListAsync();
            ValuesByObjectId = values.GroupBy(v => v.ObjectId).ToDictionary(g => g.Key, g => g.ToList());
            Console.WriteLine($"[OnGet] Existing objects loaded: {ValuesByObjectId.Count}");

            // Подготовка KeyOptions
            foreach (var field in Fields.Where(f => f.FieldType == "key" && f.IdKey.HasValue))
            {
                var targetField = await _context.Fields.Include(f => f.Table).FirstAsync(f => f.Id == field.IdKey.Value);
                var items = await _context.Values
                    .Where(v => v.TableId == targetField.TableId && v.FieldId == targetField.Id)
                    .ToListAsync();

                KeyOptions[field.Id] = items
                    .GroupBy(v => v.ObjectId)
                    .Select(g => new KeyValuePair<int, string>(g.Key, g.First().Val))
                    .ToList();

                KeyReferences[field.Id] = KeyOptions[field.Id].ToDictionary(k => k.Key, k => k.Value);
                Console.WriteLine($"[OnGet] KeyOptions for field {field.Name}: {KeyOptions[field.Id].Count}");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAddValuesAsync(int id)
        {
            Console.WriteLine($"[OnPostAddValues] Adding values for table {id}");
            Console.WriteLine($"TextValues count: {TextValues?.Count}");
            Console.WriteLine($"ImageFiles count: {ImageFiles?.Count}");
            Console.WriteLine($"KeyValues count: {KeyValues?.Count}");

            var table = await _context.Tables.Include(t => t.Fields).FirstOrDefaultAsync(t => t.Id == id);
            if (table == null)
            {
                Console.WriteLine($"[OnPostAddValues] Table {id} not found!");
                return NotFound();
            }

            int newObjectId = (_context.Values.Where(v => v.TableId == id).Select(v => v.ObjectId).DefaultIfEmpty().Max()) + 1;
            Console.WriteLine($"[OnPostAddValues] New ObjectId: {newObjectId}");

            foreach (var field in table.Fields)
            {
                var value = new EVAValue
                {
                    TableId = id,
                    FieldId = field.Id,
                    ObjectId = newObjectId
                };

                if (field.FieldType == "image")
                {
                    if (ImageFiles != null && ImageFiles.TryGetValue(field.Id, out var file) && file != null && file.Length > 0)
                    {
                        using var ms = new MemoryStream();
                        await file.CopyToAsync(ms);
                        value.BinaryValue = ms.ToArray();
                        Console.WriteLine($"[OnPostAddValues] Uploaded image for field {field.Name}, {value.BinaryValue.Length} bytes");
                    }
                }
                else if (field.FieldType == "key")
                {
                    if (KeyValues != null && KeyValues.TryGetValue(field.Id, out var key))
                    {
                        value.Val = key.ToString();
                        Console.WriteLine($"[OnPostAddValues] Key value for field {field.Name}: {value.Val}");
                    }
                }
                else
                {
                    if (TextValues != null && TextValues.TryGetValue(field.Id, out var text))
                    {
                        value.Val = text;
                        Console.WriteLine($"[OnPostAddValues] Text value for field {field.Name}: {value.Val}");
                    }
                }

                _context.Values.Add(value);
            }

            await _context.SaveChangesAsync();
            Console.WriteLine($"[OnPostAddValues] Values saved for ObjectId {newObjectId}");

            return RedirectToPage(new { id });
        }
    }
}

