﻿using System;
using System.Collections.Generic;

namespace GPS_Server.Models
{
    public partial class RequestStatus
    {
        public RequestStatus()
        {
            ConnectionRequests = new HashSet<ConnectionRequest>();
            LockRequests = new HashSet<LockRequest>();
        }

        public long StatusId { get; set; }
        public string Description { get; set; } = null!;

        public virtual ICollection<ConnectionRequest> ConnectionRequests { get; set; }
        public virtual ICollection<LockRequest> LockRequests { get; set; }
    }
}
