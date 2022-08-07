using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeApp.Entities
{
    public class Bucket 
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Name { get; set; }
        public virtual ICollection<Entry> Entries { get; set; }

    }

    public class Entry 
    {
        public int Id { get; set; }
        
        public virtual Bucket Bucket { get; set; }
        public string BucketId { get; set; }


        public DateTime Timestamp { get; set; }
        public string Value { get; set; }
    }
}