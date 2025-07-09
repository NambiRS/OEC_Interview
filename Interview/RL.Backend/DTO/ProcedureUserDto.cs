using System.ComponentModel.DataAnnotations;

namespace RL.Backend.DTO
{
    public class ProcedureUserDTO
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "ProcedureId must be greater than 0.")]
        public int ProcedureId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "UserId must be greater than 0.")]
        public int UserId { get; set; }
    }
}
