using System;
using System.Collections.Generic;
using System.Text;

namespace AgrosolutionsServiceIngestion.Domain.Entities
{
    public class Talhao
    {
        public Guid Id { get; private set; }
        public bool Active { get; private set; }

        private Talhao() { }

        public Talhao(Guid id)
        {
            Id = id;
            Active = true;
        }

        public void Disable() => Active = false;
    }
}
