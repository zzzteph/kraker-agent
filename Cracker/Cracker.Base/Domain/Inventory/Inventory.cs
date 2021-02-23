using System.Collections.Generic;
using System.Linq;
using Cracker.Base.Model;

namespace Cracker.Base.Domain.Inventory
{
    public record Inventory(IReadOnlyDictionary<string, FileDescription> Map)
    {
        public FileDescription[] Files { get; } = Map.Values.ToArray();
    }
}