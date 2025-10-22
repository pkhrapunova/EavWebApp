using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EavWebApp.Models
{
	public class EVATable
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[MaxLength(30)]
		public string Name { get; set; } = string.Empty;

		public virtual ICollection<Field> Fields { get; set; } = new List<Field>();
		public virtual ICollection<EVAValue> Values { get; set; } = new List<EVAValue>();
	}
}