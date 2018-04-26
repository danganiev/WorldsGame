using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorldsGame.Playing.Entities
{
    internal interface IEntityTemplate
    {
        /// <summary>Builds the entity.</summary>
        /// <param name="entity">The entity.</param>
        /// <param name="entityWorld">The entityWorld.</param>
        /// <param name="args">The args.</param>
        /// <returns>The build entity.</returns>
        Entity BuildEntity(EntityWorld entityWorld, params object[] args);
    }
}