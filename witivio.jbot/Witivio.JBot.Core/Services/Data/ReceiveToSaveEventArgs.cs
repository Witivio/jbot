﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witivio.JBot.Core.Services.Data
{
    public class ReceiveToSaveEventArgs<T>
    {
        public String Key { get; set; }
        public T Content { get; set; }
    }
}
