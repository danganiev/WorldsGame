﻿#region CPL License
/*
Nuclex Framework
Copyright (C) 2002-2010 Nuclex Development Labs

This library is free software; you can redistribute it and/or
modify it under the terms of the IBM Common Public License as
published by the IBM Corporation; either version 1.0 of the
License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
IBM Common Public License for more details.

You should have received a copy of the IBM Common Public
License along with this library
*/
#endregion

using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace Nuclex.UserInterface.Visuals {

  /// <summary>Interface for an exchangeable GUI painter</summary>
  public interface IGuiVisualizer {

    /// <summary>Renders an entire control tree starting at the provided control</summary>
    /// <param name="screen">Screen containing the GUI that will be drawn</param>
    void Draw(Screen screen);

  }

} // namespace Nuclex.UserInterface.Visuals
