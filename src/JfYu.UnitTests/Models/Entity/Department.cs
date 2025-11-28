#if NET8_0_OR_GREATER
using JfYu.Data.Model; 
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace JfYu.UnitTests.Models.Entity
{    
    public class Department : BaseEntity
    {
        /// <summary>
        /// Name
        /// </summary>
        [DisplayName("Name"), Required]
        public string Name { get; set; } = "";

        /// <summary>
        /// SubName
        /// </summary>
        [DisplayName("SubName"), Required]
        public string SubName { get; set; } = "";

        /// <summary>
        /// SuperiorId
        /// </summary>
        [DisplayName("SuperiorId")]
        public int? SuperiorId { get; set; }

        /// <summary>
        /// Superior
        /// </summary>
        [DisplayName("Superior")]
        public virtual Department? Superior { get; set; }

        /// <summary>
        /// Users
        /// </summary>
        [DisplayName("Users")]
        public virtual List<User>? Users { get; set; }
    }
}
#endif