using System;
using System.Collections.Generic;
using System.Text;

namespace AgrosolutionsServiceIngestion.Application.Interfaces
{
    public interface IMessageBus
    {
        void Publish<T>(T message);
    }
}
