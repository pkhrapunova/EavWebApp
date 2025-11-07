using System.Linq;
using System.Threading.Tasks;
using EavWebApp.Data;
using EavWebApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EavWebApp.Pages.Values
{
    public class ValueEditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ValueEditModel> _logger;

        public ValueEditModel(ApplicationDbContext context, ILogger<ValueEditModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public List<EVAValue> Values { get; set; } = new List<EVAValue>();

        // Убираем BindProperty для ImageFiles, будем обрабатывать вручную
        public EVATable TableInfo { get; set; }
        public List<Field> Fields { get; set; } = new List<Field>();
        public Dictionary<int, List<KeyValuePair<int, string>>> KeyOptions { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int tableId, int objectId)
        {
            _logger.LogInformation($"=== OnGetAsync: tableId={tableId}, objectId={objectId} ===");

            await LoadTableData(tableId);
            await LoadValues(tableId, objectId);
            await LoadKeyOptions();

            _logger.LogInformation($"Loaded {Fields.Count} fields and {Values.Count} values");
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
                _logger.LogInformation($"Table '{TableInfo.Name}' loaded with {Fields.Count} fields");
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
            _logger.LogInformation($"=== OnPostAsync: tableId={tableId}, objectId={objectId} ===");

            // Загружаем данные таблицы и полей
            await LoadTableData(tableId);

            if (!ModelState.IsValid)
            {
                await LoadKeyOptions();
                return Page();
            }

            try
            {
                // Обрабатываем файлы вручную
                foreach (var file in Request.Form.Files)
                {
                    _logger.LogInformation($"Processing file: {file.Name}, FieldId: {GetFieldIdFromFileName(file.Name)}");

                    var fieldId = GetFieldIdFromFileName(file.Name);
                    if (fieldId > 0 && file.Length > 0)
                    {
                        var field = Fields.FirstOrDefault(f => f.Id == fieldId);
                        if (field?.FieldType == "image")
                        {
                            _logger.LogInformation($"Saving image for field {field.Name}");

                            using var ms = new MemoryStream();
                            await file.CopyToAsync(ms);

                            var existingValue = await _context.Values
                                .FirstOrDefaultAsync(v => v.TableId == tableId &&
                                                        v.ObjectId == objectId &&
                                                        v.FieldId == fieldId);

                            if (existingValue != null)
                            {
                                existingValue.BinaryValue = ms.ToArray();
                                existingValue.Val = null;
                                _logger.LogInformation($"Updated existing image for field {field.Name}");
                            }
                            else
                            {
                                var newValue = new EVAValue
                                {
                                    TableId = tableId,
                                    FieldId = fieldId,
                                    ObjectId = objectId,
                                    BinaryValue = ms.ToArray(),
                                    Val = null
                                };
                                _context.Values.Add(newValue);
                                _logger.LogInformation($"Created new image for field {field.Name}");
                            }
                        }
                    }
                }

                // Обрабатываем текстовые значения
                foreach (var value in Values)
                {
                    var field = Fields.FirstOrDefault(f => f.Id == value.FieldId);
                    if (field?.FieldType != "image") // Пропускаем image поля, они обработаны выше
                    {
                        var existingValue = await _context.Values
                            .FirstOrDefaultAsync(v => v.TableId == tableId &&
                                                    v.ObjectId == objectId &&
                                                    v.FieldId == value.FieldId);

                        if (existingValue != null)
                        {
                            existingValue.Val = value.Val;
                            existingValue.BinaryValue = null;
                            _logger.LogInformation($"Updated text value for field {field?.Name}: '{value.Val}'");
                        }
                        else
                        {
                            value.TableId = tableId;
                            value.ObjectId = objectId;
                            value.BinaryValue = null;
                            _context.Values.Add(value);
                            _logger.LogInformation($"Created new text value for field {field?.Name}: '{value.Val}'");
                        }
                    }
                }

                var changes = await _context.SaveChangesAsync();
                _logger.LogInformation($"=== УСПЕШНО: Сохранено {changes} изменений ===");

                return RedirectToPage("/Tables/TableEdit", new { id = tableId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "=== ОШИБКА при сохранении изменений ===");
                await LoadKeyOptions();
                return Page();
            }
        }

        private int GetFieldIdFromFileName(string fileName)
        {
            // Извлекаем FieldId из имени файла "ImageFiles[4]"
            if (fileName.StartsWith("ImageFiles[") && fileName.EndsWith("]"))
            {
                var fieldIdStr = fileName.Substring(11, fileName.Length - 12);
                if (int.TryParse(fieldIdStr, out int fieldId))
                {
                    return fieldId;
                }
            }
            return 0;
        }
    }
}