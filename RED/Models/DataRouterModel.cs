﻿using RED.Interfaces;
using System.Collections.Generic;

namespace RED.Models
{
    public class DataRouterModel
    {
        internal Dictionary<ushort, List<ISubscribe>> _registrations = new Dictionary<ushort, List<ISubscribe>>();
    }
}
