﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witivio.JBot.Core.Models.ProActive.Listener
{
    public class Self
    {

        [JsonProperty("href")]
        public string Href { get; set; }
    }
}
