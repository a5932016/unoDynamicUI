using System.ComponentModel.DataAnnotations;

namespace unoDynamicUI.Models.DynamicForms;

public sealed class SampleProfileInputModel
{
    [Display(Name = "Full Name", Prompt = "Type your full name")]
    [Required]
    [StringLength(40, MinimumLength = 2)]
    public string? FullName { get; set; }

    [Display(Name = "Age", Prompt = "18 to 65")]
    [Range(18, 65)]
    public int? Age { get; set; }

    [Display(Name = "Email", Prompt = "name@example.com")]
    [EmailAddress]
    public string? Email { get; set; }

    [Display(Name = "Accept Terms")]
    [Required]
    public bool AcceptTerms { get; set; }
}
