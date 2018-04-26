using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorldsGame.Playing.Entities
{
    public interface IEntityComponent : IDisposable
    {
        // I considered having a pointer to onwer entity here, but it's a bad idea cause I'll have to initialize with owner everywhere
    }
}