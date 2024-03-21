using System.ComponentModel.DataAnnotations;

namespace CoreAdvanceConcepts.Models
{
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }

        [Required]
        [StringLength(int.MaxValue, MinimumLength = 1, ErrorMessage = "Invalid input. Please enter a valid string.")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{10,15}$", ErrorMessage = "Invalid Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(int.MaxValue, MinimumLength = 1, ErrorMessage = "Invalid input. Please enter a valid string.")]
        public string City { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Range(typeof(DateTime), "1900-01-01", "2100-01-01", ErrorMessage = "Invalid Birth date")]
        public DateTime BirthDate { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public bool FlagDeleted { get; set; }
    }
}
