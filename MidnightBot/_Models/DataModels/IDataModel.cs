﻿using SQLite;
using System;

namespace MidnightBot.DataModels
{
    internal abstract class IDataModel
    {
        [PrimaryKey, AutoIncrement]
        public int? Id { get; set; }
        [Newtonsoft.Json.JsonProperty ("createdAt")]
        public DateTime DateAdded { get; set; } = DateTime.Now;
        public IDataModel () { }
    }
}