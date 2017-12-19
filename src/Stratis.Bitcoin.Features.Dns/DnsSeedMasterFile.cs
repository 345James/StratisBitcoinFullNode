using System.IO;

namespace Stratis.Bitcoin.Features.Dns
{
    /// <summary>
    /// This class defines a DNS masterfile used to cache the whitelisted peers discovered by the DNS Seed service that supports saving
    /// and loading from a stream.
    /// </summary>
    public class DnsSeedMasterFile : MasterFile, IMasterFile
    {
        /// <summary>
        /// Loads the saved masterfile from the specified stream.
        /// </summary>
        /// <param name="stream">The stream containing the masterfile.</param>
        public void Load(Stream stream)
        {
            // TODO
        }

        /// <summary>
        /// Saves the cached masterfile to the specified stream.
        /// </summary>
        /// <param name="stream">The stream to write the masterfile to.</param>
        public void Save(Stream stream)
        {
            // TODO
        }
    }
}
