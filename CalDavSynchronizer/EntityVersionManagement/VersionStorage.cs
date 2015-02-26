﻿// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
// Copyright (c) 2015 Gerhard Zehetbauer 
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using log4net;

namespace CalDavSynchronizer.EntityVersionManagement
{
  [Serializable]
  public class VersionStorage<TEntityId, TVersion> : IVersionStorage<TEntityId, TVersion>
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod ().DeclaringType);

    private static readonly EntityIdWithVersionByIdComparer<TEntityId, TVersion> s_entityIdWithVersionByIdComparer = new EntityIdWithVersionByIdComparer<TEntityId, TVersion> (EqualityComparer<TEntityId>.Default);
    private Dictionary<TEntityId, EntityIdWithVersion<TEntityId, TVersion>> _knownVersions = new Dictionary<TEntityId, EntityIdWithVersion<TEntityId, TVersion>>();

    public Dictionary<TEntityId, EntityIdWithVersion<TEntityId, TVersion>> KnownVersionsForUnitTests
    {
      get { return _knownVersions; }
    }


    public VersionDelta<TEntityId, TVersion> SetNewVersions (IEnumerable<EntityIdWithVersion<TEntityId, TVersion>> newVersions)
    {
      var knownVersions = _knownVersions.Values;

      var deletedEvents = knownVersions.Except (newVersions, s_entityIdWithVersionByIdComparer)
          .ToArray();

      var addedEvents = newVersions.Except (knownVersions, s_entityIdWithVersionByIdComparer)
          .ToArray();

      var comparer = EqualityComparer<TVersion>.Default;

      var changedEvents = newVersions
          .Join (knownVersions, u => u.Id, u => u.Id, (newVersion, knownVersion) => new { NewVersion = newVersion, KnownVersion = knownVersion.Version })
          .Where (e => !comparer.Equals (e.NewVersion.Version, e.KnownVersion))
          .Select (e => e.NewVersion)
          .ToArray();

      _knownVersions = newVersions.ToDictionary (v => v.Id);

      return new VersionDelta<TEntityId, TVersion> (addedEvents, deletedEvents, changedEvents);
    }
   
    public void AddVersion (EntityIdWithVersion<TEntityId, TVersion> version)
    {
      _knownVersions.Add (version.Id, version);
    }

    public void ChangeVersion (EntityIdWithVersion<TEntityId, TVersion> version)
    {
      _knownVersions[version.Id] = version;
    }

    public void DeleteVersion (TEntityId entityId)
    {
      _knownVersions.Remove (entityId);
    }
  }
}