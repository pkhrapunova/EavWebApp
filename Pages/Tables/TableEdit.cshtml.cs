using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EavWebApp.Data;
using EavWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EavWebApp.Pages.Tables
{
    public class TableEditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TableEditModel> _logger;


        public TableEditModel(ApplicationDbContext context, ILogger<TableEditModel> logger)
        {
            _context = context;
            _logger = logger;
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

        public async Task<IActionResult> OnPostAddValuesAsync(int id)
        {
            _logger.LogInformation("=== НАЧАЛО OnPostAddValuesAsync ===");

            try
            {
                int newObjectId = await GetNextObjectId(id);
                var table = await _context.Tables.Include(t => t.Fields).FirstOrDefaultAsync(t => t.Id == id);

                if (table == null)
                {
                    _logger.LogError($"Table {id} not found");
                    return NotFound();
                }

                _logger.LogInformation($"Processing table: {table.Name}, New ObjectId: {newObjectId}");

                // Логируем все данные формы для отладки
                _logger.LogInformation("=== FORM DATA ===");
                foreach (var key in Request.Form.Keys)
                {
                    _logger.LogInformation($"Form[{key}] = '{Request.Form[key]}'");
                }

                _logger.LogInformation("=== FILES ===");
                foreach (var file in Request.Form.Files)
                {
                    _logger.LogInformation($"File: Name='{file.Name}', FileName='{file.FileName}', Length={file.Length}");
                }

                foreach (var field in table.Fields)
                {
                    _logger.LogInformation($"Processing field: {field.Name} (Id: {field.Id}, Type: {field.FieldType})");

                    if (field.FieldType == "image")
                    {
                        // Обработка изображений
                        var file = Request.Form.Files[$"ImageFiles[{field.Id}]"];
                        if (file != null && file.Length > 0)
                        {
                            _logger.LogInformation($"Adding image - FieldId: {field.Id}, File: {file.FileName}, Size: {file.Length} bytes");

                            using var ms = new MemoryStream();
                            await file.CopyToAsync(ms);

                            _context.Values.Add(new EVAValue
                            {
                                TableId = id,
                                FieldId = field.Id,
                                BinaryValue = ms.ToArray(),
                                ObjectId = newObjectId
                            });
                        }
                        else
                        {
                            _logger.LogWarning($"No image file for field {field.Id}");
                        }
                    }
                    else if (field.FieldType == "key")
                    {
                        // Обработка ключевых полей
                        var keyValue = Request.Form[$"KeyValues[{field.Id}]"];
                        if (!string.IsNullOrEmpty(keyValue))
                        {
                            _logger.LogInformation($"Adding key value - FieldId: {field.Id}, Value: {keyValue}");
                            _context.Values.Add(new EVAValue
                            {
                                TableId = id,
                                FieldId = field.Id,
                                Val = keyValue,
                                ObjectId = newObjectId
                            });
                        }
                        else
                        {
                            _logger.LogWarning($"No key value for field {field.Id}");
                        }
                    }
                    else
                    {
                        // Обработка текстовых полей
                        var textValue = Request.Form[$"TextValues[{field.Id}]"];
                        _logger.LogInformation($"Adding text value - FieldId: {field.Id}, Value: '{textValue}'");

                        _context.Values.Add(new EVAValue
                        {
                            TableId = id,
                            FieldId = field.Id,
                            Val = textValue,
                            ObjectId = newObjectId
                        });
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("=== УСПЕШНО: Данные сохранены ===");

                return RedirectToPage(new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "=== ОШИБКА в OnPostAddValuesAsync ===");
                throw;
            }
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