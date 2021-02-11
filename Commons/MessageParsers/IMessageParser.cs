﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons.MessageParsers
{
    public interface IMessageParser
    {
        Message ParseMessage(Message message);
    }
}
