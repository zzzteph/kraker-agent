using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Cracker.Base.Model;

namespace Cracker.Base.Domain.Inventory
{
    public record Inventory(FileDescription[] Files)
    {
        public IReadOnlyDictionary<long, FileDescription> Map { get; } = Files.ToDictionary(fd => fd.Id);
    }
}