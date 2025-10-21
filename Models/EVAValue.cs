using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace EavWebApp.Models
{
	public class EVAValue
	{
		public int Id { get; set; }

		[Column("value")]
		public string? Val { get; set; }

		[Column("object_id")]
		public int ObjectId { get; set; }

		[Column("record_field_id")]
		public int FieldId { get; set; }

		[Column("record_type_id")]
		public int TableId { get; set; }
		public Field? Field { get; set; }
		public EVATable? Table { get; set; }

		// Вспомогательные методы
		public static IQueryable<EVAValue> GetAllByObjectId(IQueryable<EVAValue> query, int objectId)
		{
			return query.Where(v => v.ObjectId == objectId);
		}

		public static IQueryable<EVAValue> GetAllByFieldId(IQueryable<EVAValue> query, int fieldId)
		{
			return query.Where(v => v.FieldId == fieldId);
		}

		public static IQueryable<EVAValue> GetAllByTableId(IQueryable<EVAValue> query, int tableId)
		{
			return query.Where(v => v.TableId == tableId);
		}
	}
}
