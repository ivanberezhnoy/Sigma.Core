using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sigma.Core.DatabaseEntity
{
    public class School
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string? ShortName { get; set; }
        public string? Number { get; set; }
        [Required]
        public uint CityId { get; set; }
        [ForeignKey("CityId")]
        [Required]
        virtual public City City { get; set; }
    }
}
