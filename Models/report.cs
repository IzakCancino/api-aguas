namespace api_aguas.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Report
    {
        public Report()
        {
            CreationDate = DateTime.Now;
            ModificationDate = DateTime.Now;
            Status = 1;
        }

        [Key]
        public int IdReport { get; set; }

        public int IdReportType { get; set; }

        public int IdUser { get; set; }

        [Required]
        public decimal Latitude { get; set; }

        [Required]
        public decimal Longitude { get; set; }

        [Required]
        [StringLength(10)]
        public string HouseNumber { get; set; }

        [Required]
        [StringLength(50)]
        public string Street { get; set; }

        [Required]
        [StringLength(50)]
        public string Neighborhood { get; set; }

        [Required]
        [StringLength(300)]
        public string Description { get; set; }

        public int Status { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime ModificationDate { get; set; }

        public virtual ReportType ReportType { get; set; }

        public virtual User User { get; set; }
    }
}
