using System.ComponentModel.DataAnnotations;

namespace _0_Framework.EntityBase
{
    public class EntityBase
    {
        [Key]
        public long Id { get; set; }

        public long? CreatorId { get; set; }

        public DateTime CreationDate { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedAt { get; set; }


        public EntityBase()
        {
            CreationDate = DateTime.Now;
            IsDeleted = false;
        }
    }
}
