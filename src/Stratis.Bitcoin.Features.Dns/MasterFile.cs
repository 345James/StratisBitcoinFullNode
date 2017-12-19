using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using Newtonsoft.Json;

namespace Stratis.Bitcoin.Features.Dns
{
    /// <summary>
    /// This class defines a master file based on 3rd party library https://github.com/kapetan/dns.
    /// </summary>
    public class MasterFile
    {
        /// <summary>
        /// Sets the default ttl.
        /// </summary>
        private static readonly TimeSpan DEFAULT_TTL = new TimeSpan(0);

        /// <summary>
        /// Identifies if the domain matches the entry.
        /// </summary>
        /// <param name="domain">The domain to match.</param>
        /// <param name="entry">The entry to match.</param>
        /// <returns><c>True</c> if there is a match, otherwise <c>false</c>.</returns>
        private static bool Matches(Domain domain, Domain entry)
        {
            string[] labels = entry.ToString().Split('.');
            string[] patterns = new string[labels.Length];

            for (int i = 0; i < labels.Length; i++)
            {
                string label = labels[i];
                patterns[i] = label == "*" ? "(\\w+)" : Regex.Escape(label);
            }

            Regex re = new Regex("^" + string.Join("\\.", patterns) + "$");
            return re.IsMatch(domain.ToString());
        }

        /// <summary>
        /// The resource record entries in the master file.
        /// </summary>
        [JsonProperty]
        private IList<IResourceRecord> entries = new List<IResourceRecord>();

        /// <summary>
        /// The default time to live.
        /// </summary>
        [JsonProperty]
        private TimeSpan ttl = DEFAULT_TTL;

        /// <summary>
        /// Initializes a new instance of a <see cref="MasterFile"/> class.
        /// </summary>
        /// <param name="ttl">The time to live.</param>
        public MasterFile(TimeSpan ttl)
        {
            this.ttl = ttl;
        }

        /// <summary>
        /// Initializes a new instance of a <see cref="MasterFile"/> class.
        /// </summary>
        public MasterFile() { }

        /// <summary>
        /// Adds a entry to the master file.
        /// </summary>
        /// <param name="entry">The resource record to add.</param>
        public void Add(IResourceRecord entry)
        {
            this.entries.Add(entry);
        }

        /// <summary>
        /// Adds an IP address resource record.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="ip">The IP address.</param>
        public void AddIPAddressResourceRecord(string domain, string ip)
        {
            this.AddIPAddressResourceRecord(new Domain(domain), IPAddress.Parse(ip));
        }

        /// <summary>
        /// Adds an IP address resource record.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="ip">The IP address.</param>
        public void AddIPAddressResourceRecord(Domain domain, IPAddress ip)
        {
            this.Add(new IPAddressResourceRecord(domain, ip, this.ttl));
        }

        /// <summary>
        /// Gets a list of matching <see cref="IResourceRecord"/> objects.
        /// </summary>
        /// <param name="domain">The domain to match on.</param>
        /// <param name="type">The type to match on.</param>
        /// <returns>The matching entries.</returns>
        public IList<IResourceRecord> Get(Domain domain, RecordType type)
        {
            return this.entries.Where(e => Matches(domain, e.Name) && e.Type == type).ToList();
        }

        /// <summary>
        /// Gets a list of matching <see cref="IResourceRecord"/> objects.
        /// </summary>
        /// <param name="question">The <see cref="Question"/>used to match on.</param>
        /// <returns>The matching entries.</returns>
        public IList<IResourceRecord> Get(Question question)
        {
            return this.Get(question.Name, question.Type);
        }
    }
}