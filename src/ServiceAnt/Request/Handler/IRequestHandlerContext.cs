﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAnt.Handler.Request.Handler
{
    public interface IRequestHandlerContext
    {
        bool IsEnd { get; set; }

        object Response { get; set; }

    }

    public class RequestHandlerContext : IRequestHandlerContext
    {
        public bool IsEnd { get; set; }

        public object Response { get; set; }
    }
}