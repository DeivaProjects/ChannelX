using System;
using System.Collections.Generic;
using ChannelX.Data;

namespace ChannelX.Models.Channel
{
    public class HistoryModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime EndAt { get; set; }
        public List<String> EngagedUsersName { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
