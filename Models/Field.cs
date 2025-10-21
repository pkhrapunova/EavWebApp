using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EavWebApp.Models
{
	public class Field
	{
		public int Id { get; set; }

		[Column("name")]
		public string Name { get; set; } = string.Empty;

		[Column("field_type")]
		public string FieldType { get; set; } = string.Empty;

		[Column("idKey")]
		public int? IdKey { get; set; }

		[Column("record_type_id")]
		public int TableId { get; set; }
		public EVATable? Table { get; set; }

		public ICollection<EVAValue> Values { get; set; } = new List<EVAValue>();
	}
}
