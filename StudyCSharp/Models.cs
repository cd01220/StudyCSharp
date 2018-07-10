using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace StudyCSharp
{
    /// <summary>
    /// the Person class.
    /// </summary>
    public class Person
    {
        /// <summary>
        /// Gets or sets the FirstName of the Person.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the LastName of the Person.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the BirthDate of the Person.
        /// </summary>
        public DateTime BirthDate { get; set; }
    }

    /// <summary>
    /// the Employee class.
    /// </summary>
    public class Employee : Person
    {
        /// <summary>
        /// Gets or sets the Department of the Employee.
        /// </summary>
        public string Department { get; set; }

        /// <summary>
        /// Gets or sets the JobTitle of the Employee.
        /// </summary>
        public string JobTitle { get; set; }
    }

    /// <summary>
    /// the Directory class.
    /// </summary>
    public class Directory
    {
        /// <summary>
        /// Gets or sets the Department of the Directory.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Parent of the Directory.
        /// </summary>
        public Directory Parent { get; set; }

        /// <summary>
        /// Gets or sets the Files of the Directory.
        /// </summary>
        public IList<File> Files { get; set; }
    }

    /// <summary>
    /// the File class.
    /// </summary>
    public class File
    {
        /// <summary>
        /// Gets or sets the Name of the File.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Parent of the File.
        /// </summary>
        public Directory Parent { get; set; }
    }

    public class TickerContainer
    {
        [JsonProperty(PropertyName = "date")]
        public DateTime Date { get; set; }

        [JsonProperty(PropertyName = "ticker")]
        public Ticker Ticker { get; set; }
    }

    public class Ticker
    {
        [JsonProperty(PropertyName = "contract_id")]
        public long ContractCid { get; set; }

        [JsonProperty(PropertyName = "high")]
        public decimal High { get; set; }

        [JsonProperty(PropertyName = "low")]
        public decimal Low { get; set; }

        [JsonProperty(PropertyName = "vol")]
        public decimal Vol { get; set; }

        [JsonProperty(PropertyName = "last")]
        public decimal Last { get; set; }

        [JsonProperty(PropertyName = "buy")]
        public decimal Buy { get; set; }

        [JsonProperty(PropertyName = "sell")]
        public decimal Sell { get; set; }
    }

    public class MarketEnt
    {
        public DateTime Time { get; set; }

        public decimal Last { get; set; }
    }
}
