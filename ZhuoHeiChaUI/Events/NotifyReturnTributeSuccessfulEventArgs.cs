﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZhuoHeiChaUI.Events
{
    public class NotifyReturnTributeSuccessfulEventArgs
    {
        public List<int> ReturnedCardIds { get; set; }
    }
}