using System;
using System.Collections.Generic;

namespace WorldsGame.Saving
{
    public interface IRuleHolder
    {
        Dictionary<int, Guid> SubruleGuids { get; }

        Dictionary<Guid, int> SubrulePriorities { get; }

        int HierarchyLevel { get; }

        void Save();
    }
}