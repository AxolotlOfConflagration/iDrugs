﻿using Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IOrderRepo
    {
        Result<IEnumerable<Order>> Get();
        Result<Order> Get(int id);
        Result Add(Order order);
        Result AddOrderItem(int orderId, OrderItem orderItem);
        Result SendOrder(int id);
    }
}