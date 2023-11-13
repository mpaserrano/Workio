using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Workio.Models
{
    public class BlockedUsersModel
    {

        public Guid Id { get; set; }

        [ForeignKey("SourceUserId")]
        public User SourceUser { get; set; }
        public string SourceUserId { get; set; }
        [ForeignKey("BlockedUserId")]
        public User BlockedUser { get; set; }
        public string BlockedUserId { get; set; }
        public DateTime BlockDateTime { get; set; } = DateTime.Now;
    }
}
