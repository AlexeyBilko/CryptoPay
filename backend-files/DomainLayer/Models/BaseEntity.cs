using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Models
{
    // Base class for all entities, providing a generic primary key
    public abstract class BaseEntity<TKey>
    {
        public TKey Id { get; set; } // Generic ID to allow different key types for different entities
    }
}
