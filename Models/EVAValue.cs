using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EavWebApp.Models
{
    [Table("FieldValue")]
    public class EVAValue
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("value")]
        [MaxLength(500)]
        public string? Val { get; set; }

        [Column("image_data")]
        public byte[]? BinaryValue { get; set; }
        [Column("object_id")]
        public int ObjectId { get; set; }

        [Column("record_field_id")]
        public int FieldId { get; set; }

        [Column("record_type_id")]
        public int TableId { get; set; }

        [ForeignKey("FieldId")]
        public virtual Field? Field { get; set; }

        [ForeignKey("TableId")]
        public virtual EVATable? Table { get; set; }
    }
}