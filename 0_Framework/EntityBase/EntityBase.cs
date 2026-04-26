using System.ComponentModel.DataAnnotations;

namespace _0_Framework.EntityBase
{
    public class EntityBase
    {
        [Key] public long Id { get; set; } 

        public long? CreatorId { get; set; }

        public DateTime CreationDate { get; set; }

        public bool IsDeleted { get; private set; }

        public DateTime? DeletedAt { get; private set; }


        public EntityBase()
        {
            CreationDate = DateTime.Now;
            IsDeleted = false;
        }

        public void SoftDelete()
        {
            IsDeleted = true;
            DeletedAt = DateTime.Now;
        }
    }
}
