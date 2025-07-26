using RL.Data.DataModels.Common;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RL.Data.DataModels
{
    public class ProcedureUser : IChangeTrackable
    {
        [Key]
        public int ProcedureUserId { get; set; }

        [ForeignKey("Procedure")]
        public int ProcedureId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        [ForeignKey("Plan")]
        public int PlanId { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }

        public virtual Procedure Procedure { get; set; }
        public virtual User User { get; set; }
        public virtual Plan Plan { get; set; }
    }
}