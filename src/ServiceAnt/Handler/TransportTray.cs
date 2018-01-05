using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAnt.Handler
{
    public abstract class TransportTray
    {
        public TransportTray()
        {
            Id = Guid.NewGuid();
            CreationDate = DateTime.UtcNow;
        }

        public Guid Id { get; }
        public DateTime CreationDate { get; }
    }

    public abstract class TransportTray<TEntity> : TransportTray
    {
        public TEntity TransportEntity { get; set; }

        public TransportTray(TEntity entity) : base()
        {
            TransportEntity = entity;
        }
    }

}
