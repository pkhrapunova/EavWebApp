using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EavWebApp.Models
{
	[Table("RecordField")]
	public class Field
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Column("name")]
		[MaxLength(100)]
		public string Name { get; set; } = string.Empty;

		[Column("field_type")]
		[MaxLength(20)]
		public string FieldType { get; set; } = string.Empty;

		[Column("idKey")]
		public int? IdKey { get; set; }

		[Column("record_type_id")]
		public int TableId { get; set; }

		[ForeignKey("TableId")]
		public virtual EVATable? Table { get; set; }

		public virtual ICollection<EVAValue> Values { get; set; } = new List<EVAValue>();
	}
}