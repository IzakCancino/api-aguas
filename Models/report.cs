namespace api_aguas.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class report
    {
        [Key]
        public int id_report { get; set; }

        public int id_report_type { get; set; }

        public int id_user { get; set; }

        public decimal latitute { get; set; }

        public decimal longitude { get; set; }

        public DateTime r_timestamp { get; set; }

        [Required]
        [StringLength(300)]
        public string r_description { get; set; }

        public virtual report_types report_types { get; set; }

        public virtual user user { get; set; }
    }
}
