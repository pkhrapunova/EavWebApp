using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EavWebApp.Models
{
	public class EVATable
	{
		public int Id { get; set; }

		[MaxLength(30)]
		public string Name { get; set; } = string.Empty;

		public ICollection<Field> Fields { get; set; } = new List<Field>();
		public ICollection<EVAValue> Values { get; set; } = new List<EVAValue>();
	}
}
