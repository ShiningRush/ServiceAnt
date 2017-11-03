using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YiBan.Common.BaseAbpModule.Events.Abstractions
{
    public class IntegrationEvent
    {
        public IntegrationEvent()
        {
            Id = Guid.NewGuid();
            CreationDate = DateTime.UtcNow;
        }

        public Guid Id { get; }
        public DateTime CreationDate { get; }
    }

    public class IntegrationEvent<TEntity> : IntegrationEvent
    {
        public TEntity TransportEntity { get; set; }

        public IntegrationEvent(TEntity entity) : base()
        {
            TransportEntity = entity;
        }
    }

}
