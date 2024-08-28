using System.ComponentModel.DataAnnotations;

namespace NewWeb.Models.Entities
{
    public class Checkout
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Amount { get; set; }
        public string Remarks { get; set; }
        public string Status { get; set; }
    }
}