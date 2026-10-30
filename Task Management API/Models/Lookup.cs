using System.ComponentModel.DataAnnotations;

namespace Task_Management_API.Models
{
    public class Lookup
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public int MajorCode { get; set; }

        [Required]
        public int MinorCode { get; set; }

        [Required]
        public required string Name { get; set; }
    }
}
